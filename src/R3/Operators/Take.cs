namespace R3
{
    public static partial class EventExtensions
    {
        public static Event<TMessage, Unit> Take<TMessage>(this Event<TMessage, Unit> source, int count)
        {
            return new Take<TMessage, Unit>(source, count, default, null);
        }

        public static Event<TMessage, TComplete> Take<TMessage, TComplete>(this Event<TMessage, TComplete> source, int count, TComplete interruptMessage)
        {
            return new Take<TMessage, TComplete>(source, count, interruptMessage, null);
        }

        public static Event<TMessage, TComplete> Take<TMessage, TComplete>(this Event<TMessage, TComplete> source, int count, Func<TComplete> interruptMessageFactory)
        {
            return new Take<TMessage, TComplete>(source, count, default!, interruptMessageFactory);
        }
    }
}

namespace R3.Operators
{
    internal sealed class Take<TMessage, TComplete>(Event<TMessage, TComplete> source, int count, TComplete interruptMessage, Func<TComplete>? interruptMessageFactory) : Event<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            return source.Subscribe(new _Take(subscriber, count, interruptMessage, interruptMessageFactory));
        }

        sealed class _Take(Subscriber<TMessage, TComplete> subscriber, int count, TComplete interruptMessage, Func<TComplete>? interruptMessageFactory) : Subscriber<TMessage, TComplete>, IDisposable
        {
            int remaining = count;

            protected override void OnNextCore(TMessage message)
            {
                if (remaining > 0)
                {
                    remaining--;
                    subscriber.OnNext(message);
                }
                else
                {
                    if (interruptMessageFactory != null)
                    {
                        subscriber.OnCompleted(interruptMessageFactory());
                    }
                    else
                    {
                        subscriber.OnCompleted(interruptMessage);
                    }
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnErrorResume(error);
            }

            protected override void OnCompletedCore(TComplete complete)
            {
                subscriber.OnCompleted(complete);
            }
        }
    }
}
