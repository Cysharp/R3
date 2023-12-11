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
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext, EventSystem.GetUnhandledExceptionHandler()));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage>(this Event<TMessage> source, Action<TMessage> onNext, Action<Exception> onErrorResume)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage>(onNext, onErrorResume));
    }

    // CompletableEvent must handle onComplete.

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, EventSystem.GetUnhandledExceptionHandler(), onComplete));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this CompletableEvent<TMessage, TComplete> source, Action<TMessage> onNext, Action<Exception> onErrorResume, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, onErrorResume, onComplete));
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
    protected override void OnNextCore(TMessage message)
    {

    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        EventSystem.GetUnhandledExceptionHandler().Invoke(error);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage>(Action<TMessage> onNext, Action<Exception> onErrorResume) : Subscriber<TMessage>
{
    [DebuggerStepThrough]
    protected override void OnNextCore(TMessage message)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<Exception> onErrorResume, Action<TComplete> onComplete) : Subscriber<TMessage, TComplete>
{
    [DebuggerStepThrough]
    protected override void OnNextCore(TMessage message)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(TComplete complete)
    {
        onComplete(complete);
    }
}
