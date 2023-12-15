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
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        for (int i = 0; i < count; i++)
        {
            observer.OnNext(value);
        }
        observer.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal sealed class RepeatC<T>(T value, int count, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }
            observer.OnNext(value);
        }
        observer.OnCompleted(default);
        return Disposable.Empty;
    }
}
