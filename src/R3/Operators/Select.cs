namespace R3;

public static partial class EventExtensions
{
    // TODO: Optimize Where.Select
    // TODO: CompletableEvent.Select
    // TODO: Element index overload

    public static Event<TResult> Select<T, TResult>(
        this Event<T> source,
        Func<T, TResult> selector)
    {
        return new Select<T, TResult>(source, selector);
    }
}

internal sealed class Select<T, TResult>(Event<T> source, Func<T, TResult> selector) : Event<TResult>
{
    protected override IDisposable SubscribeCore(Subscriber<TResult> subscriber)
    {
        return source.Subscribe(new _Select(subscriber, selector));
    }

    class _Select(Subscriber<TResult> subscriber, Func<T, TResult> selector) : Subscriber<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(selector(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }
    }
}
