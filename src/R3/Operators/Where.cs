namespace R3;

public static partial class ObservableExtensions
{
    // TODO: TState

    public static Observable<T> Where<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        if (source is Where<T> where)
        {
            // Optimize for Where.Where, create combined predicate.
            var p = where.predicate;
            return new Where<T>(where.source, x => p(x) && predicate(x));
        }

        return new Where<T>(source, predicate);
    }

    public static Observable<T> Where<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new WhereIndexed<T>(source, predicate);
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
