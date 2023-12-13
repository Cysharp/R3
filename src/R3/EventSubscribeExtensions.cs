using System.Diagnostics;

namespace R3;

public static class EventSubscribeExtensions
{
    // TODO: with State

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, TComplete> source)
    {
        return source.Subscribe(NopSubscriber<TMessage, TComplete>.Instance);
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, Result<TComplete>> source)
    {
        return source.Subscribe(NopRSubscriber<TMessage, TComplete>.Instance);
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, TComplete> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, EventSystem.GetUnhandledExceptionHandler(), Stubs<TComplete>.Nop));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, Result<TComplete>> source, Action<TMessage> onNext)
    {
        return source.Subscribe(new AnonymousRSubscriber<TMessage, TComplete>(onNext, EventSystem.GetUnhandledExceptionHandler()));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, TComplete> source, Action<TMessage> onNext, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, EventSystem.GetUnhandledExceptionHandler(), onComplete));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<TMessage, TComplete>(this Event<TMessage, TComplete> source, Action<TMessage> onNext, Action<Exception> onErrorResume, Action<TComplete> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<TMessage, TComplete>(onNext, onErrorResume, onComplete));
    }
}

[DebuggerStepThrough]
internal sealed class NopSubscriber<TMessage, TComplete> : Subscriber<TMessage, TComplete>
{
    public static readonly NopSubscriber<TMessage, TComplete> Instance = new();

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

    [DebuggerStepThrough]
    protected override void OnCompletedCore(TComplete complete)
    {
    }
}

[DebuggerStepThrough]
internal sealed class NopRSubscriber<TMessage, TComplete> : Subscriber<TMessage, Result<TComplete>>
{
    public static readonly NopRSubscriber<TMessage, TComplete> Instance = new();

    private NopRSubscriber()
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

    [DebuggerStepThrough]
    protected override void OnCompletedCore(Result<TComplete> complete)
    {
        if (complete.IsFailure)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(complete.Exception);
        }
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

[DebuggerStepThrough]
internal sealed class AnonymousRSubscriber<TMessage, TComplete>(Action<TMessage> onNext, Action<Exception> onErrorResume) : Subscriber<TMessage, Result<TComplete>>
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
    protected override void OnCompletedCore(Result<TComplete> complete)
    {
        if (complete.IsFailure)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(complete.Exception);
        }
    }
}
