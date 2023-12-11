namespace R3;

public sealed class CancellationDisposable(CancellationTokenSource cancellationTokenSource) : IDisposable
{
    public CancellationDisposable()
        : this(new CancellationTokenSource())
    {
    }

    public CancellationToken Token => cancellationTokenSource.Token;

    public bool IsDisposed => cancellationTokenSource.IsCancellationRequested;

    public void Dispose() => cancellationTokenSource.Cancel();
}
