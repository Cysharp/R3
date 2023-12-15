namespace R3;

public static partial class Observable
{
    public static Observable<T> ToEvent<T>(this Task<T> task)
    {
        return new TaskToEvent<T>(task);
    }

    public static Observable<T> ToEvent<T>(this IEnumerable<T> source)
    {
        return new EnumerableToEvent<T>(source);
    }
}

internal sealed class TaskToEvent<T>(Task<T> task) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        var subscription = new CancellationDisposable();
        SubscribeTask(subscriber, subscription.Token);
        return subscription;
    }

    async void SubscribeTask(Observer<T> subscriber, CancellationToken cancellationToken)
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

internal class EnumerableToEvent<T>(IEnumerable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(message);
        }
        subscriber.OnCompleted();
        return Disposable.Empty;
    }
}
