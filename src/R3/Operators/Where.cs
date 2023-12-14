namespace R3;

public static partial class EventExtensions
{
    public static Event<T> Where<T>(this Event<T> source, Func<TMessage, bool> predicate)
    {
        if (source is Where<T> where)
        {
            // Optimize for Where.Where, create combined predicate.
            var p = where.predicate;
            return new Where<T>(where.source, x => p(x) && predicate(x));
        }

        return new Where<T>(source, predicate);
    }

    public static Event<T> Where<T>(this Event<T> source, Func<TMessage, int, bool> predicate)
    {
        return new WhereIndexed<T>(source, predicate);
    }
}

internal sealed class Where<T>(Event<T> source, Func<TMessage, bool> predicate) : Event<T>
{
    internal Event<T> source = source;
    internal Func<TMessage, bool> predicate = predicate;

    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(Subscriber<T> subscriber, Func<TMessage, bool> predicate) : Subscriber<T>
    {
        protected override void OnNextCore(T value)
        {
            if (predicate(message))
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(complete);
        }
    }
}

internal sealed class WhereIndexed<T>(Event<T> source, Func<TMessage, int, bool> predicate) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _Where(subscriber, predicate));
    }

    class _Where(Subscriber<T> subscriber, Func<TMessage, int, bool> predicate) : Subscriber<T>
    {
        int index = 0;

        protected override void OnNextCore(T value)
        {
            if (predicate(message, index++))
            {
                subscriber.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(complete);
        }
    }
}
