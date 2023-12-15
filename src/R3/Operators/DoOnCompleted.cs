namespace R3;

public static partial class EventExtensions
{
    // TODO: more accurate impl
    // TODO: with state


    // TODO: other file.
    public static Observable<T> CancelOnCompleted<T>(this Observable<T> source, CancellationTokenSource cancellationTokenSource)
    {
        return new DoOnCompleted<T>(source, _ => cancellationTokenSource.Cancel());
    }


    public static Observable<T> DoOnCompleted<T>(this Observable<T> source, Action<Result> action)
    {
        return new DoOnCompleted<T>(source, action);
    }
}

internal sealed class DoOnCompleted<T>(Observable<T> source, Action<Result> action) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        var method = new _DoOnCompleted(subscriber, action);
        source.Subscribe(method);
        return method;
    }

    class _DoOnCompleted(Observer<T> subscriber, Action<Result> action) : Observer<T>, IDisposable
    {
        Action<Result>? action = action;

        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            Interlocked.Exchange(ref action, null)?.Invoke(result);
            subscriber.OnCompleted(result);
        }
    }
}
