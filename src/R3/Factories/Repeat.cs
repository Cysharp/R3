namespace R3;

public static partial class Event
{
    // no scheduler(TimeProvider) overload
    // no infinitely overload

    public static Event<TMessage, Unit> Repeat<TMessage>(TMessage value, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<TMessage>();
        }

        return new Repeat<TMessage>(value, count);
    }

    public static Event<TMessage, Unit> Repeat<TMessage>(TMessage value, int count, CancellationToken cancellationToken)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<TMessage>();
        }

        return new RepeatC<TMessage>(value, count, cancellationToken);
    }
}

internal sealed class Repeat<TMessage>(TMessage value, int count) : Event<TMessage, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            subscriber.OnNext(value);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal sealed class RepeatC<TMessage>(TMessage value, int count, CancellationToken cancellationToken) : Event<TMessage, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Disposable.Empty;
            }
            subscriber.OnNext(value);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}
