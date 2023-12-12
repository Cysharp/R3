
namespace R3.Internal;

// for return Task(tcs.TrySet***)
// include proper Cancel registration

internal abstract class TaskSubscriberBase<TMessage, TTask> : Subscriber<TMessage>
{
    protected TaskCompletionSource<TTask> tcs; // use this field.

    CancellationToken cancellationToken;
    CancellationTokenRegistration tokenRegistration;

    public Task<TTask> Task => tcs.Task;

    public TaskSubscriberBase(CancellationToken cancellationToken)
    {
        this.tcs = new TaskCompletionSource<TTask>();
        this.cancellationToken = cancellationToken;

        if (cancellationToken.CanBeCanceled)
        {
            this.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (TaskSubscriberBase<TMessage, TTask>)state!;

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
}

internal abstract class TaskSubscriberBase<TMessage, TComplete, TTask> : Subscriber<TMessage, TComplete>
{
    protected TaskCompletionSource<TTask> tcs; // use this field.

    CancellationToken cancellationToken;
    CancellationTokenRegistration tokenRegistration;

    public Task<TTask> Task => tcs.Task;

    public TaskSubscriberBase(CancellationToken cancellationToken)
    {
        this.tcs = new TaskCompletionSource<TTask>();
        this.cancellationToken = cancellationToken;

        if (cancellationToken.CanBeCanceled)
        {
            this.tokenRegistration = cancellationToken.UnsafeRegister(static state =>
            {
                var s = (TaskSubscriberBase<TMessage, TComplete, TTask>)state!;

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
}
