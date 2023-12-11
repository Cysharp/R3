namespace R3;

public sealed class SingleAssignmentDisposable : IDisposable
{
    SingleAssignmentDisposableCore core;

    public bool IsDisposed => core.IsDisposed;

    public IDisposable? Disposable
    {
        get => core.Disposable;
        set => core.Disposable = value;
    }

    public void Dispose()
    {
        core.Dispose();
    }
}
