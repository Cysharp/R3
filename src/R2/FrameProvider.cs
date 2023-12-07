using R2.Internal;

namespace R2;

public abstract class FrameProvider
{
    public abstract int GetFrameCount();
    public abstract IFrameTimer CreateTimer(TimerCallback callback, object? state, int initialFrame, int frameInterval);

    public int GetElapsedFrameCount(int startingFrame) => GetElapsedFrameCount(startingFrame, GetFrameCount());

    public int GetElapsedFrameCount(int startingFrame, int endingFrame)
    {
        // TODO: validate
        var frameCount = GetFrameCount();
        return endingFrame - startingFrame;
    }
}

public interface IFrameTimer : IDisposable, IAsyncDisposable
{
    bool Change(int initialFrame, int frameInterval);
}

public static class FrameProviderExtensions
{
    public static IFrameTimer CreateStoppedTimer(this FrameProvider frameProvider, TimerCallback timerCallback, object? state)
    {
        return frameProvider.CreateTimer(timerCallback, state, -1, -1);
    }

    public static void RestartImmediately(this IFrameTimer timer)
    {
        timer.Change(0, -1);
    }

    public static void InvokeOnce(this IFrameTimer timer, int initialFrame)
    {
        timer.Change(initialFrame, -1);
    }
}

public interface IFrameTimerWorkItem
{
    // true, continue
    bool Execute();
}

public sealed class ThreadFrameProvider : FrameProvider
{
    public override int GetFrameCount()
    {
        return GlobalThreadSleepLoop.Instance.FrameCount;
    }

    public override IFrameTimer CreateTimer(TimerCallback callback, object? state, int initialFrame, int frameInterval)
    {
        var timer = new ThreadSleepFrameTimer(callback, state);
        timer.Change(initialFrame, frameInterval);
        return timer;
    }
}

public sealed class ThreadSleepFrameTimer : IFrameTimer, IFrameTimerWorkItem
{
    readonly TimerCallback timerCallback;
    readonly object? state;

    int frameCount;
    State runningState;
    int initialFrame;
    int frameInterval;

    public ThreadSleepFrameTimer(TimerCallback timerCallback, object? state)
    {
        this.timerCallback = timerCallback;
        this.state = state;
    }

    public bool Change(int initialFrame, int frameInterval)
    {
        // TODO: lock.
        this.initialFrame = initialFrame;
        this.frameInterval = frameInterval;

        if (initialFrame != -1 && runningState != State.Stop)
        {
            GlobalThreadSleepLoop.Instance.Register(this);
        }
        else
        {
            runningState = State.Stop;
        }

        return true;
    }

    public bool Execute()
    {
        switch (runningState)
        {
            case State.Stop:
                return false;
            case State.Initial:
                goto INITIAL;
            case State.Interval:
                goto INTERVAL;
            default:
                break;
        }

        frameCount = 0;

    INITIAL:
        if (frameCount++ == initialFrame)
        {
            timerCallback(state);
            if (frameInterval == -1)
            {
                runningState = State.Stop;
                return false;
            }
            else
            {
                frameCount = 0;
                runningState = State.Interval;
            }
        }
        return true;

    INTERVAL:
        if (frameCount++ == frameInterval)
        {
            timerCallback(state);
            frameCount = 0;
        }
        return true;
    }





    public void Dispose()
    {
        // TODO:...
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    enum State
    {
        Stop,
        Initial,
        Interval
    }
}

internal sealed class GlobalThreadSleepLoop
{
    public static readonly GlobalThreadSleepLoop Instance = new GlobalThreadSleepLoop();

    public int FrameCount;
    FreeListCore<IFrameTimerWorkItem> list;

    GlobalThreadSleepLoop()
    {
        list = new FreeListCore<IFrameTimerWorkItem>(this);
        Run();
    }

    public void Register(IFrameTimerWorkItem callback)
    {
        list.Add(callback);
    }

    void Run()
    {
        while (true)
        {
            var span = list.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                ref readonly var item = ref span[i];
                if (item != null)
                {
                    if (!item.Execute()) // TODO:try-catch
                    {
                        list.Remove(i);
                    }
                }
            }

            Thread.Sleep(1);
            FrameCount++;
        }
    }
}
