namespace R3.Internal;

// for return Task(tcs.TrySet***)
// include proper Cancel registration

internal abstract class TaskSubscriberBase<T, TTask> : Subscriber<T>
{
    TaskCompletionSource<TTask> tcs; // use this field.

    CancellationToken cancellationToken;
    CancellationTokenRegistration tokenRegistration;

    public Task<TTask> Task => tcs.Task;

    public TaskSubscriberBase(CancellationToken cancellationToken)
    {
        this.tcs = new TaskCompletionSource<TTask>();
        this.cancellationToken = cancellationToken;

        if (cancellationToken.CanBeCanceled)
        {
            // register before call Subscribe
            this.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (TaskSubscriberBase<T, TTask>)state!;

                s.Dispose(); // subscriber is subscription, dispose
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
