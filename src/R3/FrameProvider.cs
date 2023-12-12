namespace R3;

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

public sealed class ThreadSleepFrameProvider : FrameProvider, IDisposable
{
    readonly int sleepMilliseconds;
    bool disposed;

    long frameCount;
    FreeListCore<IFrameRunnerWorkItem> list;
    Thread thread;

    public ThreadSleepFrameProvider()
        : this(1)
    {
    }

    public ThreadSleepFrameProvider(int sleepMilliseconds)
    {
        this.sleepMilliseconds = sleepMilliseconds;
        this.list = new FreeListCore<IFrameRunnerWorkItem>(this);
        this.thread = new Thread(Run);
        this.thread.Start();
    }

    public override long GetFrameCount()
    {
        ObjectDisposedException.ThrowIf(disposed, typeof(ThreadSleepFrameProvider));
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        ObjectDisposedException.ThrowIf(disposed, typeof(ThreadSleepFrameProvider));
        list.Add(callback);
    }

    public void Dispose()
    {
        disposed = true;
    }

    void Run()
    {
        while (!disposed)
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
                            OnUnhandledException(ex);
                        }
                        catch { }
                    }
                }
            }

            Thread.Sleep(sleepMilliseconds);
            frameCount++;
        }
        list.Dispose();
    }
}
