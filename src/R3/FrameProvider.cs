namespace R3;

public abstract class FrameProvider
{
    public abstract long GetFrameCount();
    public abstract void Register(IFrameRunnerWorkItem callback);
}

public interface IFrameRunnerWorkItem
{
    // true, continue
    bool MoveNext(long frameCount);
}

public sealed class FakeFrameProvider : FrameProvider
{
    long frameCount;
    readonly object gate = new object();
    FreeListCore<IFrameRunnerWorkItem> list;

    public FakeFrameProvider()
    {
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        this.frameCount = 0;
    }

    public FakeFrameProvider(long frameCount)
    {
        this.list = new FreeListCore<IFrameRunnerWorkItem>(gate);
        this.frameCount = frameCount;
    }

    public override long GetFrameCount()
    {
        return frameCount;
    }

    public override void Register(IFrameRunnerWorkItem callback)
    {
        list.Add(callback, out _);
    }

    public void Advance()
    {
        Advance(1);
    }

    public void Advance(int advanceCount)
    {
        for (int i = 0; i < advanceCount; i++)
        {
            RunLoop();
        }
    }

    public int GetRegisteredCount()
    {
        var span = list.AsSpan();
        var count = 0;
        foreach (ref readonly var item in span)
        {
            if (item != null) count++;
        }
        return count;
    }

    void RunLoop()
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
                        ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                    }
                    catch { }
                }
            }
        }
        frameCount++;
    }
}
