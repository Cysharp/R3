namespace R3.Internal;

// when Canceled, publish OnCompleted.
internal abstract class CancellableFrameRunnerWorkItemBase<T> : IFrameRunnerWorkItem, IDisposable
{
    protected readonly Observer<T> observer;
    CancellationTokenRegistration cancellationTokenRegistration;
    bool isDisposed;

    public CancellableFrameRunnerWorkItemBase(Observer<T> observer, CancellationToken cancellationToken)
    {
        this.observer = observer;

        if (cancellationToken.CanBeCanceled)
        {
            this.cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (CancellableFrameRunnerWorkItemBase<T>)state!;
                s.observer.OnCompleted();
                s.Dispose();
            }, this);
        }
    }

    public bool MoveNext(long frameCount)
    {
        if (isDisposed)
        {
            return false;
        }

        return MoveNextCore();
    }

    protected abstract bool MoveNextCore();

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            cancellationTokenRegistration.Dispose();
            DisposeCore();
        }
    }

    protected virtual void DisposeCore() { }
}
