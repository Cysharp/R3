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

public interface IThreadPoolWorkItem
{
    //
    // 概要:
    //     Executes the work item on the thread pool.
    void Execute();
}

// Thread.Sleep(1)

public sealed class ThreadSleepTimer : IFrameTimer
{



    public bool Change(int initialFrame, int frameInterval)
    {
        // IThreadPoolWorkItem
        //ThreadPool.UnsafeQueueUserWorkItem(

        // CompactListCore


        throw new NotImplementedException();
    }







    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

internal sealed class GlobalThreadSleepLoop
{
    public int FrameCount;


    public void Run()
    {
        while (true)
        {



            Thread.Sleep(1);
            FrameCount++;
        }
    }
}




internal sealed class TaskYieldFrameProvider
{
    List<(Func<object?, bool> callback, object? state)> list1 = new();
    Task? loop;

    public void Register(Func<object?, bool> callback, object? state)
    {
        list1.Add((callback, state));
        if (loop == null)
        {
            loop = TaskLoop();
        }
    }

    async Task TaskLoop()
    {
        while (true) // TODO: how stop?
        {
            // TODO: thread safety

            foreach (var item in list1.ToArray()) // TODO: avoid ToArray()
            {
                if (!item.callback(item.state))
                {
                    list1.Remove(item); // TODO: slow Remove
                }
            }

            await Task.Yield();
        }
    }
}
