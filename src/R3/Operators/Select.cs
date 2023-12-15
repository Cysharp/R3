namespace R3;

public static partial class EventExtensions
{
    // TODO: Optimize Where.Select
    // TODO: Element index overload

    public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, TResult> selector)
    {
        return new Select<T, TResult>(source, selector);
    }

    public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state, Func<T, TState, TResult> selector)
    {
        return new Select<T, TResult, TState>(source, selector, state);
    }
}

internal sealed class Select<T, TResult>(Observable<T> source, Func<T, TResult> selector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> subscriber)
    {
        return source.Subscribe(new _Select(subscriber, selector));
    }

    class _Select(Observer<TResult> subscriber, Func<T, TResult> selector) : Observer<T>
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

internal sealed class Select<T, TResult, TState>(Observable<T> source, Func<T, TState, TResult> selector, TState state) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> subscriber)
    {
        return source.Subscribe(new _Select(subscriber, selector, state));
    }

    class _Select(Observer<TResult> subscriber, Func<T, TState, TResult> selector, TState state) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(selector(value, state));
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
