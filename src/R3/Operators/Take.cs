namespace R3;

public static partial class EventExtensions
{
    public static Event<T> Take<T>(this Event<T> source, int count)
    {
        return new Take<T>(source, count);
    }
}

internal sealed class Take<T>(Event<T> source, int count) : Event<T>
{
    protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
    {
        return source.Subscribe(new _Take(subscriber, count));
    }

    sealed class _Take(Subscriber<T> subscriber, int count) : Subscriber<T>, IDisposable
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
                subscriber.OnCompleted();
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            subscriber.OnCompleted(result);
        }
    }
}
