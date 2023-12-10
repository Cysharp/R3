namespace R3;

public static partial class EventExtensions
{
    public static Event<TMessage> Where<TMessage>(this Event<TMessage> source, Func<TMessage, bool> predicate)
    {
        return new Where<TMessage>(source, predicate);
    }
}

internal class Where<TMessage>(Event<TMessage> source, Func<TMessage, bool> predicate) : Event<TMessage>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(Subscriber<TMessage> subscriber, Func<TMessage, bool> predicate) : Subscriber<TMessage>
    {
        public override void OnNext(TMessage message)
        {
            if (predicate(message))
            {
                subscriber.OnNext(message);
            }
        }
    }
}
