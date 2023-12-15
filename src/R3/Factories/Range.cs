namespace R3;

public static partial class Observable
{
    // no scheduler(TimeProvider) overload

    public static Observable<int> Range(int start, int count)
    {
        long max = ((long)start) + count - 1;
        if (count < 0 || max > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<int>();
        }

        return new Range(start, count);
    }

    public static Observable<int> Range(int start, int count, CancellationToken cancellationToken)
    {
        long max = ((long)start) + count - 1;
        if (count < 0 || max > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<int>();
        }

        return new RangeC(start, count, cancellationToken);
    }
}

internal sealed class Range(int start, int count) : Observable<int>
{
    protected override IDisposable SubscribeCore(Observer<int> observer)
    {
        for (int i = 0; i < count; i++)
        {
            observer.OnNext(start + i);
        }
        observer.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal sealed class RangeC(int start, int count, CancellationToken cancellationToken) : Observable<int>
{
    protected override IDisposable SubscribeCore(Observer<int> observer)
    {
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }
            observer.OnNext(start + i);
        }
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
