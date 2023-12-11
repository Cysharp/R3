namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: Optimize Where.Select
        // TODO: CompletableEvent.Select
        // TODO: Element index overload

        // TODO: Select for TComplete

        public static Event<TResult> Select<TMessage, TResult>(this Event<TMessage> source, Func<TMessage, TResult> selector)
        {
            return new Select<TMessage, TResult>(source, selector);
        }

        public static CompletableEvent<TMessageResult, TCompleteResult> Select<TMessage, TComplete, TMessageResult, TCompleteResult>(
            this CompletableEvent<TMessage, TComplete> source,
            Func<TMessage, TMessageResult> messageSelector,
            Func<TComplete, TCompleteResult> completeSelector)
        {
            return new Select<TMessage, TComplete, TMessageResult, TCompleteResult>(source, messageSelector, completeSelector);
        }
    }
}

namespace R3.Operators
{
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
    
    internal sealed class Select<TMessage, TComplete, TMessageResult, TCompleteResult>(
        CompletableEvent<TMessage, TComplete> source,
           Func<TMessage, TMessageResult> messageSelector,
            Func<TComplete, TCompleteResult> completeSelector
        ) : CompletableEvent<TMessageResult, TCompleteResult>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessageResult, TCompleteResult> subscriber)
        {
            return source.Subscribe(new _Select(subscriber, messageSelector, completeSelector));
        }

        class _Select(Subscriber<TMessageResult, TCompleteResult> subscriber, Func<TMessage, TMessageResult> messageSelector, Func<TComplete, TCompleteResult> completeSelector) : Subscriber<TMessage, TComplete>
        {
            public override void OnNext(TMessage message)
            {
                subscriber.OnNext(messageSelector(message));
            }

            protected override void OnCompletedCore(TComplete complete)
            {
                subscriber.OnCompleted(completeSelector(complete));
            }
        }
    }
}
