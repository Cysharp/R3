namespace R3
{
    public static partial class EventExtensions
    {
        public static Event<TMessage, TComplete> Where<TMessage, TComplete>(this Event<TMessage, TComplete> source, Func<TMessage, bool> predicate)
        {
            if (source is Where<TMessage, TComplete> where)
            {
                // Optimize for Where.Where, create combined predicate.
                var p = where.predicate;
                return new Where<TMessage, TComplete>(where.source, x => p(x) && predicate(x));
            }

            return new Where<TMessage, TComplete>(source, predicate);
        }

        public static Event<TMessage, TComplete> Where<TMessage, TComplete>(this Event<TMessage, TComplete> source, Func<TMessage, int, bool> predicate)
        {
            return new WhereIndexed<TMessage, TComplete>(source, predicate);
        }
    }
}

namespace R3.Operators
{
    internal sealed class Where<TMessage, TComplete>(Event<TMessage, TComplete> source, Func<TMessage, bool> predicate) : Event<TMessage, TComplete>
    {
        internal Event<TMessage, TComplete> source = source;
        internal Func<TMessage, bool> predicate = predicate;

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

    internal sealed class WhereIndexed<TMessage, TComplete>(Event<TMessage, TComplete> source, Func<TMessage, int, bool> predicate) : Event<TMessage, TComplete>
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
