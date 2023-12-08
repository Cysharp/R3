using R2.Internal;

namespace R2;

public abstract class FrameProvider
{
    protected event Action<Exception>? UnhandledException;
    public abstract long GetFrameCount();
    public abstract void Register(IFrameRunnerWorkItem callback);

    protected void OnUnhandledException(Exception ex)
    {
        UnhandledException?.Invoke(ex);
    }
}

public interface IFrameRunnerWorkItem
{
    // true, continue
    bool MoveNext(long frameCount);
}

public sealed class ThreadFrameProvider : FrameProvider
{
    public static readonly ThreadFrameProvider Instance = new ThreadFrameProvider();

    ThreadFrameProvider()
    {
    }

    // Start GlobalThreadSleepLoop after first touch.

    public override long GetFrameCount()
    {
        return GlobalThreadSleepFrameRunner.Instance.FrameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        GlobalThreadSleepFrameRunner.Instance.Register(callback);
    }

    internal void RaiseUnhandledException(Exception ex)
    {
        OnUnhandledException(ex);
    }
}

internal sealed class GlobalThreadSleepFrameRunner
{
    public static readonly GlobalThreadSleepFrameRunner Instance = new GlobalThreadSleepFrameRunner();

    long frameCount;
    FreeListCore<IFrameRunnerWorkItem> list;
    Thread thread;

    GlobalThreadSleepFrameRunner()
    {
        list = new FreeListCore<IFrameRunnerWorkItem>(this);
        thread = new Thread(Run);
        thread.Start();
    }

    public long FrameCount => frameCount;

    public void Register(IFrameRunnerWorkItem callback)
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
                    try
                    {
                        if (!item.MoveNext(frameCount))
                        {
                            list.Remove(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        list.Remove(i);
                        try
                        {
                            ThreadFrameProvider.Instance.RaiseUnhandledException(ex);
                        }
                        catch { }
                    }
                }
            }

            Thread.Sleep(1); // TODO: non-static, configurable?
            frameCount++;
        }
    }
}
