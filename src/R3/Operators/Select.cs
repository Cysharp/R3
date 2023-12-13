namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: Optimize Where.Select
        // TODO: CompletableEvent.Select
        // TODO: Element index overload

        // TODO: Select for TComplete


        public static Event<TMessageResult, TComplete> Select<TMessage, TComplete, TMessageResult>(
            this Event<TMessage, TComplete> source,
            Func<TMessage, TMessageResult> messageSelector)
        {
            return new Select<TMessage, TComplete, TMessageResult, TComplete>(source, messageSelector, Stubs<TComplete>.ReturnSelf);
        }

        public static Event<TMessageResult, TCompleteResult> Select<TMessage, TComplete, TMessageResult, TCompleteResult>(
            this Event<TMessage, TComplete> source,
            Func<TMessage, TMessageResult> messageSelector,
            Func<TComplete, TCompleteResult> completeSelector)
        {
            return new Select<TMessage, TComplete, TMessageResult, TCompleteResult>(source, messageSelector, completeSelector);
        }
    }
}

namespace R3.Operators
{
    internal sealed class Select<TMessage, TComplete, TMessageResult, TCompleteResult>(
        Event<TMessage, TComplete> source,
           Func<TMessage, TMessageResult> messageSelector,
            Func<TComplete, TCompleteResult> completeSelector
        ) : Event<TMessageResult, TCompleteResult>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessageResult, TCompleteResult> subscriber)
        {
            return source.Subscribe(new _Select(subscriber, messageSelector, completeSelector));
        }

        class _Select(Subscriber<TMessageResult, TCompleteResult> subscriber, Func<TMessage, TMessageResult> messageSelector, Func<TComplete, TCompleteResult> completeSelector) : Subscriber<TMessage, TComplete>
        {
            protected override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(messageSelector(message));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnErrorResume(error);
            }

            protected override void OnCompletedCore(TComplete complete)
            {
                subscriber.OnCompleted(completeSelector(complete));
            }
        }
    }
}
