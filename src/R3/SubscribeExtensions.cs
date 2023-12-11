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
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext, EventSystem.UnhandledException));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage>(this Event<TMessage> source, Action<TMessage> onNext, Action<Exception> onError)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext, onError));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return Subscribe(source, onNext, static _ => { });
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, EventSystem.UnhandledException, onComplete));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<Exception> onError, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, onError, onComplete));
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
    public override void OnNextCore(TMessage message)
    {

    }

    [DebuggerStepThrough]
    public override void OnErrorCore(Exception error)
    {
        EventSystem.UnhandledException(error);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage>(Action<TMessage> onNext, Action<Exception> onError) : Subscriber<TMessage>
{
    [DebuggerStepThrough]
    public override void OnNextCore(TMessage message)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    public override void OnErrorCore(Exception error)
    {
        onError(error);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<Exception> onError, Action<TComplete> onComplete) : Subscriber<TMessage, TComplete>
{
    [DebuggerStepThrough]
    public override void OnNextCore(TMessage message)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    public override void OnErrorCore(Exception error)
    {
        onError(error);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(TComplete complete)
    {
        onComplete(complete);
    }
}
