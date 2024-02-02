namespace R3;

public sealed class BooleanDisposable : IDisposable
{
    BooleanDisposableCore core;

    public bool IsDisposed => core.IsDisposed;

    public void Dispose()
    {
        core.Dispose();
    }
}

public struct BooleanDisposableCore
{
    int isDisposed;

    public bool IsDisposed => Volatile.Read(ref isDisposed) == 1;

    public void Dispose()
    {
        Volatile.Write(ref isDisposed, 1);
    }
}
