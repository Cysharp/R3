using System.Security.AccessControl;
using System.Threading;
using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, TResult> selector)
    {
        if (source is Where<T> where)
        {
            // Optimize for WhereSelect
            return new WhereSelect<T, TResult>(where.source, selector, where.predicate);
        }

        return new Select<T, TResult>(source, selector);
    }

    public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, int, TResult> selector)
    {
        return new SelectIndexed<T, TResult>(source, selector);
    }

    // TState

    public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state, Func<T, TState, TResult> selector)
    {
        return new Select<T, TResult, TState>(source, selector, state);
    }

    public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state, Func<T, int, TState, TResult> selector)
    {
        return new SelectIndexed<T, TResult, TState>(source, selector, state);
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
        return source.Subscribe(new _WhereSelect(observer, selector, predicate));
    }

    sealed class _WhereSelect(Observer<TResult> observer, Func<T, TResult> selector, Func<T, bool> predicate) : Observer<T>
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

internal sealed class SelectIndexed<T, TResult>(Observable<T> source, Func<T, int, TResult> selector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Select(observer, selector));
    }

    sealed class _Select(Observer<TResult> observer, Func<T, int, TResult> selector) : Observer<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            observer.OnNext(selector(value, index++));
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

internal sealed class SelectIndexed<T, TResult, TState>(Observable<T> source, Func<T, int, TState, TResult> selector, TState state) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Select(observer, selector, state));
    }

    sealed class _Select(Observer<TResult> observer, Func<T, int, TState, TResult> selector, TState state) : Observer<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            observer.OnNext(selector(value, index++, state));
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
