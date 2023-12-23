using System.Threading;

namespace R3.Internal;

// for return Task(tcs.TrySet***)
// include proper Cancel registration

internal abstract class TaskObserverBase<T, TTask> : Observer<T>
{
    TaskCompletionSource<TTask> tcs; // use this field.

    CancellationToken cancellationToken;
    CancellationTokenRegistration tokenRegistration;

    public Task<TTask> Task => tcs.Task;

    public TaskObserverBase(CancellationToken cancellationToken)
    {
        this.tcs = new TaskCompletionSource<TTask>();
        this.cancellationToken = cancellationToken;

        if (cancellationToken.CanBeCanceled)
        {
            // register before call Subscribe
            this.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (TaskObserverBase<T, TTask>)state!;

                s.Dispose(); // observer is subscription, dispose
                s.tcs.TrySetCanceled(s.cancellationToken);
            }, this);
        }
    }

    // if override, should call base.DisposeCore(), be careful.
    protected override void DisposeCore()
    {
        tokenRegistration.Dispose();
    }

    protected void TrySetResult(TTask result)
    {
        try
        {
            tcs.TrySetResult(result);
        }
        finally
        {
            Dispose();
        }
    }

    protected void TrySetException(Exception exception)
    {
        try
        {
            tcs.TrySetException(exception);
        }
        finally
        {
            Dispose();
        }
    }
}
