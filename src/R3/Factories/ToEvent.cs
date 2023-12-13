namespace R3;

public static partial class Event
{
    public static Event<TMessage, Unit> ToEvent<TMessage>(this IEnumerable<TMessage> source)
    {
        return new ToEvent<TMessage>(source);
    }
}

internal class ToEvent<TMessage>(IEnumerable<TMessage> source) : Event<TMessage, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(message);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}
