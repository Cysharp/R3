namespace R3;

public static partial class Event
{
    public static Event<TMessage> ToCompletableEvent<T>(this Task<T> task)
    {
        return new ToCompletableEvent<T>(task);
    }
}

internal sealed class ToCompletableEvent<T>(Task<T> task) : Event<TMessage>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        var subscription = new CancellationDisposable();
        SubscribeTask(subscriber, subscription.Token);
        return subscription;
    }

    async void SubscribeTask(Subscriber<TMessage> subscriber, CancellationToken cancellationToken)
    {
        TMessage? result;
        try
        {
            result = await task.WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            subscriber.OnCompleted(Result.Failure(ex));
            return;
        }

        subscriber.OnNext(result);
        subscriber.OnCompleted(Result.Success<Unit>(default));
    }
}
