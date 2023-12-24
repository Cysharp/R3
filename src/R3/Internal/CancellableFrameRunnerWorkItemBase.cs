namespace R3.Internal;

// when Canceled, publish OnCompleted.
internal abstract class CancellableFrameRunnerWorkItemBase<T> : IFrameRunnerWorkItem, IDisposable
{
    readonly Observer<T> observer;
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

        if (observer.IsDisposed)
        {
            Dispose();
            return false;
        }

        return MoveNextCore(frameCount);
    }

    protected abstract bool MoveNextCore(long frameCount);

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

    // Observer

    protected void PublishOnNext(T value)
    {
        observer.OnNext(value);
    }

    protected void PublishOnErrorResume(Exception error)
    {
        observer.OnErrorResume(error);
    }

    protected void PublishOnCompleted(Exception error)
    {
        observer.OnCompleted(error);
        Dispose();
    }

    protected void PublishOnCompleted()
    {
        observer.OnCompleted();
        Dispose();
    }
}
