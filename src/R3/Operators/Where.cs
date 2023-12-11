namespace R3
{
    public static partial class EventExtensions
    {
        public static Event<TMessage> Where<TMessage>(this Event<TMessage> source, Func<TMessage, bool> predicate)
        {
            if (source is Where<TMessage> where)
            {
                // Optimize for Where.Where, create combined predicate.
                var p = where.predicate;
                return new Where<TMessage>(where.source, x => p(x) && predicate(x));
            }

            return new Where<TMessage>(source, predicate);
        }

        public static CompletableEvent<TMessage, TComplete> Where<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate)
        {
            return new Where<TMessage, TComplete>(source, predicate);
        }

        public static Event<TMessage> Where<TMessage>(this Event<TMessage> source, Func<TMessage, int, bool> predicate)
        {
            return new WhereIndexed<TMessage>(source, predicate);
        }

        public static CompletableEvent<TMessage, TComplete> Where<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Func<TMessage, int, bool> predicate)
        {
            return new WhereIndexed<TMessage, TComplete>(source, predicate);
        }
    }
}

namespace R3.Operators
{
    internal sealed class Where<TMessage>(Event<TMessage> source, Func<TMessage, bool> predicate) : Event<TMessage>
    {
        internal Event<TMessage> source = source;
        internal Func<TMessage, bool> predicate = predicate;

        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            return source.Subscribe(new _Where(subscriber, predicate));
        }

        class _Where(Subscriber<TMessage> subscriber, Func<TMessage, bool> predicate) : Subscriber<TMessage>
        {
            protected override void OnNextCore(TMessage message)
            {
                if (predicate(message))
                {
                    subscriber.OnNext(message);
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnErrorResume(error);
            }
        }
    }

    internal sealed class Where<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, Func<TMessage, bool> predicate) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            return source.Subscribe(new _Where(subscriber, predicate));
        }

        class _Where(Subscriber<TMessage, TComplete> subscriber, Func<TMessage, bool> predicate) : Subscriber<TMessage, TComplete>
        {
            protected override void OnNextCore(TMessage message)
            {
                if (predicate(message))
                {
                    subscriber.OnNext(message);
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

    internal sealed class WhereIndexed<TMessage>(Event<TMessage> source, Func<TMessage, int, bool> predicate) : Event<TMessage>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            return source.Subscribe(new _Where(subscriber, predicate));
        }

        class _Where(Subscriber<TMessage> subscriber, Func<TMessage, int, bool> predicate) : Subscriber<TMessage>
        {
            int index = 0;

            protected override void OnNextCore(TMessage message)
            {
                if (predicate(message, index++))
                {
                    subscriber.OnNext(message);
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                subscriber.OnErrorResume(error);
            }
        }
    }

    internal sealed class WhereIndexed<TMessage, TComplete>(CompletableEvent<TMessage, TComplete> source, Func<TMessage, int, bool> predicate) : CompletableEvent<TMessage, TComplete>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            return source.Subscribe(new _Where(subscriber, predicate));
        }

        class _Where(Subscriber<TMessage, TComplete> subscriber, Func<TMessage, int, bool> predicate) : Subscriber<TMessage, TComplete>
        {
            int index = 0;

            protected override void OnNextCore(TMessage message)
            {
                if (predicate(message, index++))
                {
                    subscriber.OnNext(message);
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
