namespace R3;

public static partial class EventExtensions
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
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _TakeWhile(subscriber, predicate));
    }

    sealed class _TakeWhile(Observer<T> subscriber, Func<T, bool> predicate) : Observer<T>, IDisposable
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                subscriber.OnNext(value);
            }
            else
            {
                subscriber.OnCompleted();
            }
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

internal sealed class TakeWhileI<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _TakeWhile(subscriber, predicate));
    }

    sealed class _TakeWhile(Observer<T> subscriber, Func<T, int, bool> predicate) : Observer<T>, IDisposable
    {
        int count;

        protected override void OnNextCore(T value)
        {
            if (predicate(value, count++))
            {
                subscriber.OnNext(value);
            }
            else
            {
                subscriber.OnCompleted();
            }
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
