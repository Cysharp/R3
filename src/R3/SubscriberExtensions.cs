namespace R3;

public static class SubscriberExtensions
{
    public static void OnCompleted<T>(this Subscriber<T, Unit> subscriber)
    {
        subscriber.OnCompleted(default);
    }

    public static IObserver<T> ToObserver<T>(this Subscriber<T> subscriber)
    {
        return new R3Observer<T>(subscriber);
    }

    public static IObserver<T> ToObserver<T>(this Subscriber<T, Unit> subscriber)
    {
        return new R3Observer2<T>(subscriber);
    }

    public static IObserver<T> ToObserver<T>(this Subscriber<T, Result<Unit>> subscriber)
    {
        return new R3Observer3<T>(subscriber);
    }
}

internal sealed class R3Observer<T>(Subscriber<T> subscriber) : IObserver<T>
{
    public void OnNext(T value)
    {
        subscriber.OnNext(value);
    }

    public void OnError(Exception error)
    {
        try
        {
            subscriber.OnErrorResume(error);
        }
        finally
        {
            subscriber.Dispose();
        }
    }

    public void OnCompleted()
    {
        subscriber.Dispose();
    }
}

internal sealed class R3Observer2<T>(Subscriber<T, Unit> subscriber) : IObserver<T>
{
    public void OnNext(T value)
    {
        subscriber.OnNext(value);
    }

    public void OnError(Exception error)
    {
        try
        {
            subscriber.OnErrorResume(error);
        }
        finally
        {
            subscriber.Dispose();
        }
    }

    public void OnCompleted()
    {
        subscriber.OnCompleted(default);
    }
}

internal sealed class R3Observer3<T>(Subscriber<T, Result<Unit>> subscriber) : IObserver<T>
{
    public void OnNext(T value)
    {
        subscriber.OnNext(value);
    }

    public void OnError(Exception error)
    {
        subscriber.OnCompleted(Result.Failure<Unit>(error));
    }

    public void OnCompleted()
    {
        subscriber.OnCompleted(Result.Success(Unit.Default));
    }
}
