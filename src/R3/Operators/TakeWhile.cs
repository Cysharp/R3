namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> TakeWhile<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        return new TakeWhile<T>(source, predicate);
    }

    public static Observable<T> TakeWhile<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new TakeWhileI<T>(source, predicate);
    }
}

internal sealed class TakeWhile<T>(Observable<T> source, Func<T, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeWhile(observer, predicate));
    }

    sealed class _TakeWhile(Observer<T> observer, Func<T, bool> predicate) : Observer<T>, IDisposable
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                observer.OnNext(value);
            }
            else
            {
                observer.OnCompleted();
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

internal sealed class TakeWhileI<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeWhile(observer, predicate));
    }

    sealed class _TakeWhile(Observer<T> observer, Func<T, int, bool> predicate) : Observer<T>, IDisposable
    {
        int count;

        protected override void OnNextCore(T value)
        {
            if (predicate(value, count++))
            {
                observer.OnNext(value);
            }
            else
            {
                observer.OnCompleted();
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
