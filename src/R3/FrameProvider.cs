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
