namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: more accurate impl
        // TODO: with state
        public static CompletableEvent<TMessage, TComplete> DoOnCompleted<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action action)
        {
            return new DoOnCompleted<TMessage, TComplete>(source, action);
        }
    }
}

namespace R3.Operators
{
    internal sealed class DoOnCompleted<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, Action action) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            var method = new _DoOnCompleted(subscriber, action);
            source.Subscribe(method);
            return method;
        }

        class _DoOnCompleted(Subscriber<TMessage, TComplete> subscriber, Action action) : Subscriber<TMessage, TComplete>, IDisposable
        {
            Action? action = action;

            protected override void OnNextCore(TMessage message)
            {
                subscriber.OnNext(message);
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnErrorResume(error);
            }

            protected override void OnCompletedCore(TComplete complete)
            {
                Interlocked.Exchange(ref action, null)?.Invoke();
                subscriber.OnCompleted(complete);
            }
        }
    }
}
