namespace R2;

public static class EventFactory
{
    public static ICompletableEvent<TMessage, Unit> ToEvent<TMessage>(this IEnumerable<TMessage> source)
    {
        return new EnumerableToEvent<TMessage>(source);
    }
}

internal class EnumerableToEvent<TMessage>(IEnumerable<TMessage> source) : ICompletableEvent<TMessage, Unit>
{
    public IDisposable Subscribe(ISubscriber<TMessage, Unit> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(message);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}