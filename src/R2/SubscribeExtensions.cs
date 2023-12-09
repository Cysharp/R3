using System.Diagnostics;

namespace R2;

public static class SubscribeExtensions
{
    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage>(this Event<TMessage> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return Subscribe(source, onNext, _ => { });
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, onComplete));
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage>(Action<TMessage> onNext) : Subscriber<TMessage>
{
    public override void OnNext(TMessage message)
    {
        onNext(message);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<TComplete> onComplete) : Subscriber<TMessage, TComplete>
{
    public override void OnNext(TMessage message)
    {
        onNext(message);
    }

    protected override void OnCompletedCore(TComplete complete)
    {
        onComplete(complete);
    }
}
