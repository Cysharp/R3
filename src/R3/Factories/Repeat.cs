namespace R3;

public static partial class Observable
{
    // no scheduler(TimeProvider) overload
    // no infinitely overload

    public static Observable<T> Repeat<T>(T value, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<T>();
        }

        return new Repeat<T>(value, count);
    }

    public static Observable<T> Repeat<T>(T value, int count, CancellationToken cancellationToken)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<T>();
        }

        return new RepeatC<T>(value, count, cancellationToken);
    }
}

internal sealed class Repeat<T>(T value, int count) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            subscriber.OnNext(value);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal sealed class RepeatC<T>(T value, int count, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                subscriber.OnCompleted();
                return Disposable.Empty;
            }
            subscriber.OnNext(value);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}
