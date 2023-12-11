namespace R3
{
    public static partial class EventExtensions
    {
        public static CompletableEvent<TMessage, Result<Unit>> OnErrorAsComplete<TMessage>(this Event<TMessage> source)
        {
            return new OnErrorAsComplete<TMessage>(source);
        }

        public static CompletableEvent<TMessage, Result<TComplete>> OnErrorAsComplete<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source)
        {
            return new OnErrorAsComplete<TMessage, TComplete>(source);
        }
    }
}

namespace R3.Operators
{
    internal class OnErrorAsComplete<TMessage>(Event<TMessage> source) : CompletableEvent<TMessage, Result<Unit>>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, Result<Unit>> subscriber)
        {
            return source.Subscribe(new _OnErrorAsComplete(subscriber));
        }

        sealed class _OnErrorAsComplete(Subscriber<TMessage, Result<Unit>> subscriber) : Subscriber<TMessage>
        {
            protected override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnCompleted(Result.Failure<Unit>(error));
            }
        }
    }

    internal class OnErrorAsComplete<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source) : CompletableEvent<TMessage, Result<TComplete>>
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
}
