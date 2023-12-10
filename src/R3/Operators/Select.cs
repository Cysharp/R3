namespace R3;

public static partial class EventExtensions
{
    public static Event<TResult> Select<TMessage, TResult>(this Event<TMessage> source, Func<TMessage, TResult> selector)
    {
        return new Select<TMessage, TResult>(source, selector);
    }
}

internal sealed class Select<TMessage, TResult>(Event<TMessage> source, Func<TMessage, TResult> selector) : Event<TResult>
{
    protected override IDisposable SubscribeCore(Subscriber<TResult> subscriber)
    {
        return source.Subscribe(new _Select(subscriber, selector));
    }

    class _Select(Subscriber<TResult> subscriber, Func<TMessage, TResult> selector) : Subscriber<TMessage>
    {
        public override void OnNext(TMessage message)
        {
            subscriber.OnNext(selector(message));
        }
    }
}
