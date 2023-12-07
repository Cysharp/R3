namespace R2;

public static partial class EventExtensions
{
    public static IEvent<TMessage> Where<TMessage>(this IEvent<TMessage> source, Func<TMessage, bool> predicate)
    {
        return new Where<TMessage>(source, predicate);
    }
}

internal class Where<TMessage>(IEvent<TMessage> source, Func<TMessage, bool> predicate) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(ISubscriber<TMessage> subscriber, Func<TMessage, bool> predicate) : ISubscriber<TMessage>
    {
        public void OnNext(TMessage message)
        {
            if (predicate(message))
            {
                subscriber.OnNext(message);
            }
        }
    }
}
