namespace R3;

public static partial class ObservableExtensions
{
    // TODO: Element index overload

    public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, TResult> selector)
    {
        if (source is Where<T> where)
        {
            return new WhereSelect<T, TResult>(source, selector, where.predicate);
        }

        return new Select<T, TResult>(source, selector);
    }

    public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state, Func<T, TState, TResult> selector)
    {
        return new Select<T, TResult, TState>(source, selector, state);
    }
}

internal sealed class Select<T, TResult>(Observable<T> source, Func<T, TResult> selector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Select(observer, selector));
    }

    sealed class _Select(Observer<TResult> observer, Func<T, TResult> selector) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(selector(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}

internal sealed class Select<T, TResult, TState>(Observable<T> source, Func<T, TState, TResult> selector, TState state) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Select(observer, selector, state));
    }

    sealed class _Select(Observer<TResult> observer, Func<T, TState, TResult> selector, TState state) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(selector(value, state));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}

internal sealed class WhereSelect<T, TResult>(Observable<T> source, Func<T, TResult> selector, Func<T, bool> predicate) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Select(observer, selector, predicate));
    }

    sealed class _Select(Observer<TResult> observer, Func<T, TResult> selector, Func<T, bool> predicate) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                observer.OnNext(selector(value));
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}
