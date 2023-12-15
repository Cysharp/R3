namespace R3;

public static class SubscriberExtensions
{
    public static void OnCompleted<T>(this Observer<T> subscriber)
    {
        subscriber.OnCompleted(Result.Success);
    }

    public static void OnCompleted<T>(this Observer<T> subscriber, Exception exception)
    {
        subscriber.OnCompleted(Result.Failure(exception));
    }

    public static IObserver<T> ToObserver<T>(this Observer<T> subscriber)
    {
        return new SubscriberToObserver<T>(subscriber);
    }
}

internal sealed class SubscriberToObserver<T>(Observer<T> subscriber) : IObserver<T>
{
    public void OnNext(T value)
    {
        subscriber.OnNext(value);
    }

    public void OnError(Exception error)
    {
        subscriber.OnCompleted(error);
    }

    public void OnCompleted()
    {
        subscriber.OnCompleted();
    }
}
