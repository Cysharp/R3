namespace R3;

public static partial class EventExtensions
{
    public static Event<T> OnErrorResumeAsFailure<T>(this Event<T> source)
    {
        return new OnErrorResumeAsFailure<T>(source);
    }
}

internal class OnErrorResumeAsFailure<T>(Event<T> source) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _OnErrorAsComplete(subscriber));
    }

    sealed class _OnErrorAsComplete(Subscriber<T> subscriber) : Subscriber<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnCompleted(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(complete);
        }
    }
}
