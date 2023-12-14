namespace R3;

public static partial class EventExtensions
{
    public static Event<TMessage, Result<Result>> OnErrorAsComplete<T>(this Event<T> source)
    {
        return new OnErrorAsComplete<T>(source);
    }
}

internal class OnErrorAsComplete<T>(Event<T> source) : Event<TMessage, Result<Result>>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Result<Result>> subscriber)
    {
        return source.Subscribe(new _OnErrorAsComplete(subscriber));
    }

    sealed class _OnErrorAsComplete(Subscriber<TMessage, Result<Result>> subscriber) : Subscriber<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnCompleted(Result.Failure<Result>(error));
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(Result.Success(complete));
        }
    }
}
