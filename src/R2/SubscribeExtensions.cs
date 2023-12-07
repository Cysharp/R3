namespace R2;

public static class SubscribeExtensions
{
    public static IDisposable Subscribe<TMessage>(this IEvent<TMessage> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new Subscriber<TMessage>(onNext));
    }

    public static IDisposable Subscribe<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new Subscriber<TMessage, TComplete>(onNext, _ => { }));
    }

    public static IDisposable Subscribe<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new Subscriber<TMessage, TComplete>(onNext, onComplete));
    }
}

internal sealed class Subscriber<TMessage>(Action<TMessage> onNext) : ISubscriber<TMessage>
{
    public void OnNext(TMessage message)
    {
        onNext(message);
    }
}

internal sealed class Subscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<TComplete> onComplete) : ISubscriber<TMessage, TComplete>
{
    public void OnNext(TMessage message)
    {
        onNext(message);
    }

    public void OnCompleted(TComplete complete)
    {
        onComplete(complete);
    }
}