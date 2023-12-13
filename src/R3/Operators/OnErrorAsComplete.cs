namespace R3;

public static partial class EventExtensions
{
    public static Event<TMessage, Result<TComplete>> OnErrorAsComplete<TMessage, TComplete>(this Event<TMessage, TComplete> source)
    {
        return new OnErrorAsComplete<TMessage, TComplete>(source);
    }
}

internal class OnErrorAsComplete<TMessage, TComplete>(Event<TMessage, TComplete> source) : Event<TMessage, Result<TComplete>>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Result<TComplete>> subscriber)
    {
        return source.Subscribe(new _OnErrorAsComplete(subscriber));
    }

    sealed class _OnErrorAsComplete(Subscriber<TMessage, Result<TComplete>> subscriber) : Subscriber<TMessage, TComplete>
    {
        protected override void OnNextCore(TMessage message)
        {
            subscriber.OnNext(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnCompleted(Result.Failure<TComplete>(error));
        }

        protected override void OnCompletedCore(TComplete complete)
        {
            subscriber.OnCompleted(Result.Success(complete));
        }
    }
}
