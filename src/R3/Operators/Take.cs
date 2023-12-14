namespace R3;

public static partial class EventExtensions
{
    public static Event<TMessage> Take<T>(this Event<TMessage> source, int count)
    {
        return new Take<TMessage>(source, count, default, null);
    }

    public static Event<T> Take<T>(this Event<T> source, int count interruptMessage)
    {
        return new Take<T>(source, count, interruptMessage, null);
    }

    public static Event<T> Take<T>(this Event<T> source, int count, Func<Result> interruptMessageFactory)
    {
        return new Take<T>(source, count, default!, interruptMessageFactory);
    }
}

internal sealed class Take<T>(Event<T> source, int count interruptMessage, Func<Result>? interruptMessageFactory) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _Take(subscriber, count, interruptMessage, interruptMessageFactory));
    }

    sealed class _Take(Subscriber<T> subscriber, int count interruptMessage, Func<Result>? interruptMessageFactory) : Subscriber<T>, IDisposable
    {
        int remaining = count;

        protected override void OnNextCore(T value)
        {
            if (remaining > 0)
            {
                remaining--;
                subscriber.OnNext(value);
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

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(complete);
        }
    }
}
