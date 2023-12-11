using System.Diagnostics;

namespace R3;

public static class SubscribeExtensions
{
    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage>(this Event<TMessage> source)
    {
        return source.Subscribe(NopSubscriber<TMessage>.Instance);
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage>(this Event<TMessage> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext));
    }

    // TODO: Result<>, failer throw?

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return Subscribe(source, onNext, static _ => { });
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, onComplete));
    }
}

[DebuggerStepThrough]
internal sealed class NopSubscriber<TMessage> : Subscriber<TMessage>
{
    public static readonly NopSubscriber<TMessage> Instance = new();

    private NopSubscriber()
    {
    }

    [DebuggerStepThrough]
    public override void OnNext(TMessage message)
    {
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage>(Action<TMessage> onNext) : Subscriber<TMessage>
{
    [DebuggerStepThrough]
    public override void OnNext(TMessage message)
    {
        onNext(message);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<TComplete> onComplete) : Subscriber<TMessage, TComplete>
{
    [DebuggerStepThrough]
    public override void OnNext(TMessage message)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(TComplete complete)
    {
        onComplete(complete);
    }
}
