namespace R3;

public static partial class Event
{
    public static Event<T> ToEvent<T>(this Task<T> task)
    {
        return new TaskToEvent<T>(task);
    }

    public static Event<T> ToEvent<T>(this IEnumerable<T> source)
    {
        return new EnumerableToEvent<T>(source);
    }
}

internal sealed class TaskToEvent<T>(Task<T> task) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var subscription = new CancellationDisposable();
        SubscribeTask(subscriber, subscription.Token);
        return subscription;
    }

    async void SubscribeTask(Subscriber<T> subscriber, CancellationToken cancellationToken)
    {
        T? result;
        try
        {
            result = await task.WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken) // disposed.
            {
                return;
            }

            subscriber.OnCompleted(ex);
            return;
        }

        subscriber.OnNext(result);
        subscriber.OnCompleted();
    }
}

internal class EnumerableToEvent<T>(IEnumerable<T> source) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(message);
        }
        subscriber.OnCompleted();
        return Disposable.Empty;
    }
}
