namespace R2;

public sealed class CallbackDisposable<T> : IDisposable
{
    readonly Action<T> onDisposed;

    int isDisposed;
    T state;

    public bool IsDisposed => isDisposed == 1;

    public CallbackDisposable(Action<T> onDisposed, T state)
    {
        this.onDisposed = onDisposed;
        this.state = state;
    }

    public void Dispose()
    {
        var locationValue = Interlocked.CompareExchange(ref isDisposed, 1, 0);
        if (locationValue == 0)
        {
            onDisposed.Invoke(state);
        }
    }
}
