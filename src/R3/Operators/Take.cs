namespace R3;

public static partial class EventExtensions
{
    public static CompletableEvent<TMessage, Unit> Take<TMessage>(this Event<TMessage> source, int count)
    {
        return new Take<TMessage>(source, count);
    }

    //public static ICompletableEvent<TMessage, TComplete> Delay<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, TimeSpan dueTime, TimeProvider timeProvider)
    //{
    //    return new Delay<TMessage, TComplete>(source, dueTime, timeProvider);
    //}
}

internal sealed class Take<TMessage>(Event<TMessage> source, int count) : CompletableEvent<TMessage, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
    {
        return source.Subscribe(new _Take(subscriber, count));
    }

    sealed class _Take(Subscriber<TMessage, Unit> subscriber, int count) : Subscriber<TMessage>, IDisposable
    {
        int remaining = count;

        public override void OnNext(TMessage message)
        {
            if (remaining > 0)
            {
                remaining--;
                subscriber.OnNext(message);
            }
            else
            {
                subscriber.OnCompleted(Unit.Default);
            }
        }
    }
}
