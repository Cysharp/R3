namespace R3
{
    public static partial class EventExtensions
    {
        public static CompletableEvent<TMessage, Unit> Take<TMessage>(this Event<TMessage> source, int count)
        {
            return new Take<TMessage>(source, count);
        }

        public static CompletableEvent<TMessage, Unit> Take<TMessage>(this CompletableEvent<TMessage, Unit> source, int count)
        {
            return new Take<TMessage, Unit>(source, count, default, null);
        }

        public static CompletableEvent<TMessage, TComplete> Take<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, int count, TComplete interruptMessage)
        {
            return new Take<TMessage, TComplete>(source, count, interruptMessage, null);
        }

        public static CompletableEvent<TMessage, TComplete> Take<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, int count, Func<TComplete> interruptMessageFactory)
        {
            return new Take<TMessage, TComplete>(source, count, default!, interruptMessageFactory);
        }
    }
}

namespace R3.Operators
{
    internal sealed class Take<TMessage>(Event<TMessage> source, int count) : CompletableEvent<TMessage, Unit>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
        {
            return source.Subscribe(new _Take(subscriber, count));
        }

        sealed class _Take(Subscriber<TMessage, Unit> subscriber, int count) : Subscriber<TMessage>, IDisposable
        {
            int remaining = count;

            public override void OnNext(TMessage message)
            {
                if (remaining > 0)
                {
                    remaining--;
                    subscriber.OnNext(message);
                }
                else
                {
                    subscriber.OnCompleted(Unit.Default);
                }
            }
        }
    }

    internal sealed class Take<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, int count, TComplete interruptMessage, Func<TComplete>? interruptMessageFactory) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            return source.Subscribe(new _Take(subscriber, count, interruptMessage, interruptMessageFactory));
        }

        sealed class _Take(Subscriber<TMessage, TComplete> subscriber, int count, TComplete interruptMessage, Func<TComplete>? interruptMessageFactory) : Subscriber<TMessage, TComplete>, IDisposable
        {
            int remaining = count;

            public override void OnNext(TMessage message)
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

            protected override void OnCompletedCore(TComplete complete)
            {
                subscriber.OnCompleted(complete);
            }
        }
    }
}
