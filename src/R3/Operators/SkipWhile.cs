namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> SkipWhile<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        return new SkipWhile<T>(source, predicate);
    }

    public static Observable<T> SkipWhile<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new SkipWhileI<T>(source, predicate);
    }
}

internal sealed class SkipWhile<T>(Observable<T> source, Func<T, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipWhile(observer, predicate));
    }

    sealed class _SkipWhile(Observer<T> observer, Func<T, bool> predicate) : Observer<T>, IDisposable
    {
        bool open;

        protected override void OnNextCore(T value)
        {
            if (open)
            {
                observer.OnNext(value);
            }
            else if (!predicate(value))
            {
                open = true;
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

internal sealed class SkipWhileI<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipWhile(observer, predicate));
    }

    sealed class _SkipWhile(Observer<T> observer, Func<T, int, bool> predicate) : Observer<T>, IDisposable
    {
        int count;
        bool open;

        protected override void OnNextCore(T value)
        {
            if (open)
            {
                observer.OnNext(value);
            }
            else if (!predicate(value, count++))
            {
                open = true;
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
