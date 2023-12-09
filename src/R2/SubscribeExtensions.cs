namespace R2;

public static class SubscribeExtensions
{
    public static IDisposable Subscribe<TMessage>(this IEvent<TMessage> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new Subscriber<TMessage>(onNext));
    }

    public static IDisposable Subscribe<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return Subscribe(source, onNext, _ => { });
    }

    public static IDisposable Subscribe<TMessage, TComplete>(this ICompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        var subscriber = new Subscriber<TMessage, TComplete>(onNext, onComplete);
        subscriber.SourceSubscription.Disposable = source.Subscribe(subscriber);
        return subscriber;
    }
}

internal sealed class Subscriber<TMessage>(Action<TMessage> onNext) : ISubscriber<TMessage>
{
    public void OnNext(TMessage message)
    {
        onNext(message);
    }
}

internal sealed class Subscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<TComplete> onComplete) : ISubscriber<TMessage, TComplete>, IDisposable
{
    public SingleAssignmentDisposableCore SourceSubscription;

    public void OnNext(TMessage message)
    {
        onNext(message);
    }

    // auto detach
    public void OnCompleted(TComplete complete)
    {
        try
        {
            onComplete(complete);
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        SourceSubscription.Dispose();
    }
}
