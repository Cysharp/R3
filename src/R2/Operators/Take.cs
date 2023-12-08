namespace R2;

public static partial class EventExtensions
{
    public static ICompletableEvent<TMessage, Unit> Take<TMessage>(this IEvent<TMessage> source, int count)
    {
        return new Take<TMessage>(source, count);
    }

    //public static ICompletableEvent<TMessage, TComplete> Delay<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, TimeSpan dueTime, TimeProvider timeProvider)
    //{
    //    return new Delay<TMessage, TComplete>(source, dueTime, timeProvider);
    //}
}

internal sealed class Take<TMessage>(IEvent<TMessage> source, int count) : ICompletableEvent<TMessage, Unit>
{
    public IDisposable Subscribe(ISubscriber<TMessage, Unit> subscriber)
    {
        var take = new _Take(subscriber, count);
        take.SourceSubscription.Disposable = source.Subscribe(take);
        return take;
    }

    class _Take(ISubscriber<TMessage, Unit> subscriber, int count) : ISubscriber<TMessage>, IDisposable
    {
        public SingleAssignmentDisposableCore SourceSubscription = new SingleAssignmentDisposableCore();

        int onNextCount;

        public void OnNext(TMessage message)
        {
            if (count == onNextCount++)
            {
                try
                {
                    subscriber.OnCompleted(Unit.Default);
                }
                finally
                {
                    Dispose();
                }
                return;
            }

            subscriber.OnNext(message);
        }

        public void Dispose()
        {
            SourceSubscription.Dispose();
        }
    }
}