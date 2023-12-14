namespace R3;

public static partial class Event
{
    public static Event<TMessage> ToEvent<T>(this IEnumerable<T> source)
    {
        return new ToEvent<T>(source);
    }
}

internal class ToEvent<T>(IEnumerable<T> source) : Event<TMessage>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(value);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}
