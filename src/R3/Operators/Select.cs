namespace R3;

public static partial class EventExtensions
{
    // TODO: Optimize Where.Select
    // TODO: CompletableEvent.Select
    // TODO: Element index overload

    // TODO: Select for Result


    public static Event<TMessageResult> Select<TMessage, TMessageResult>(
        this Event<T> source,
        Func<TMessage, TMessageResult> messageSelector)
    {
        return new Select<TMessage, TMessageResult>(source, messageSelector, Stubs<Result>.ReturnSelf);
    }

    public static Event<TMessageResultResult> Select<TMessage, TMessageResultResult>(
        this Event<T> source,
        Func<TMessage, TMessageResult> messageSelector,
        Func<ResultResult> completeSelector)
    {
        return new Select<TMessage, TMessageResultResult>(source, messageSelector, completeSelector);
    }
}

internal sealed class Select<TMessage, TMessageResultResult>(
    Event<T> source,
       Func<TMessage, TMessageResult> messageSelector,
        Func<ResultResult> completeSelector
    ) : Event<TMessageResultResult>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessageResultResult> subscriber)
    {
        return source.Subscribe(new _Select(subscriber, messageSelector, completeSelector));
    }

    class _Select(Subscriber<TMessageResultResult> subscriber, Func<TMessage, TMessageResult> messageSelector, Func<ResultResult> completeSelector) : Subscriber<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(messageSelector(message));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(completeSelector(complete));
        }
    }
}
