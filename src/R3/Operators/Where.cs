namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Where<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        if (source is Where<T> where)
        {
            // Optimize for Where.Where, create combined predicate.
            var p = where.predicate;
            return new Where<T>(where.source, x => p(x) && predicate(x)); // lambda captured but don't use TState to allow combine more Where
        }

        return new Where<T>(source, predicate);
    }

    public static Observable<T> Where<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new WhereIndexed<T>(source, predicate);
    }

    // TState

    public static Observable<T> Where<T, TState>(this Observable<T> source, TState state, Func<T, TState, bool> predicate)
    {
        return new Where<T, TState>(source, predicate, state);
    }

    public static Observable<T> Where<T, TState>(this Observable<T> source, TState state, Func<T, int, TState, bool> predicate)
    {
        return new WhereIndexed<T, TState>(source, predicate, state);
    }
}

internal sealed class Where<T>(Observable<T> source, Func<T, bool> predicate) : Observable<T>
{
    internal Observable<T> source = source;
    internal Func<T, bool> predicate = predicate; // use in WhereWhere, WhereSelect(Select.cs)

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Where(observer, predicate));
    }

    class _Where(Observer<T> observer, Func<T, bool> predicate) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                observer.OnNext(value);
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

internal sealed class WhereIndexed<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Where(observer, predicate));
    }

    class _Where(Observer<T> observer, Func<T, int, bool> predicate) : Observer<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            if (predicate(value, index++))
            {
                observer.OnNext(value);
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

internal sealed class Where<T, TState>(Observable<T> source, Func<T, TState, bool> predicate, TState state) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Where(observer, predicate, state));
    }

    class _Where(Observer<T> observer, Func<T, TState, bool> predicate, TState state) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value, state))
            {
                observer.OnNext(value);
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

internal sealed class WhereIndexed<T, TState>(Observable<T> source, Func<T, int, TState, bool> predicate, TState state) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Where(observer, predicate, state));
    }

    class _Where(Observer<T> observer, Func<T, int, TState, bool> predicate, TState state) : Observer<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            if (predicate(value, index++, state))
            {
                observer.OnNext(value);
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
