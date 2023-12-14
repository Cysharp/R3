namespace R3;

public static partial class EventExtensions
{
    // TODO: more accurate impl
    // TODO: with state


    // TODO: other file.
    public static Event<T> CancelOnCompleted<T>(this Event<T> source, CancellationTokenSource cancellationTokenSource)
    {
        return new DoOnCompleted<T>(source, _ => cancellationTokenSource.Cancel());
    }


    public static Event<T> DoOnCompleted<T>(this Event<T> source, Action<Result> action)
    {
        return new DoOnCompleted<T>(source, action);
    }
}

internal sealed class DoOnCompleted<T>(Event<T> source, Action<Result> action) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        var method = new _DoOnCompleted(subscriber, action);
        source.Subscribe(method);
        return method;
    }

    class _DoOnCompleted(Subscriber<T> subscriber, Action<Result> action) : Subscriber<T>, IDisposable
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
