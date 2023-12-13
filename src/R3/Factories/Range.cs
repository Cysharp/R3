namespace R3;

public static partial class Event
{
    // no scheduler(TimeProvider) overload

    public static Event<int, Unit> Range(int start, int count)
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

    public static Event<int, Unit> Range(int start, int count, CancellationToken cancellationToken)
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

internal sealed class Range(int start, int count) : Event<int, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<int, Unit> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            subscriber.OnNext(start + i);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal sealed class RangeC(int start, int count, CancellationToken cancellationToken) : Event<int, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<int, Unit> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Disposable.Empty;
            }
            subscriber.OnNext(start + i);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}
