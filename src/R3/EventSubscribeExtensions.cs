using System.Diagnostics;

namespace R3;

public static class EventSubscribeExtensions
{
    // TODO: with State

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source)
    {
        return source.Subscribe(NopSubscriber<T>.Instance);
    }


    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source, Action<T> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<T>(onNext, EventSystem.GetUnhandledExceptionHandler(), Stubs.HandleError));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source, Action<T> onNext, Action<Result> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<T>(onNext, EventSystem.GetUnhandledExceptionHandler(), onComplete));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source, Action<T> onNext, Action<Exception> onErrorResume, Action<Result> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<T>(onNext, onErrorResume, onComplete));
    }
}

[DebuggerStepThrough]
internal sealed class NopSubscriber<T> : Subscriber<T>
{
    public static readonly NopSubscriber<T> Instance = new();

    private NopSubscriber()
    {
    }

    [DebuggerStepThrough]
    protected override void OnNextCore(T value)
    {
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        EventSystem.GetUnhandledExceptionHandler().Invoke(error);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(result.Exception);
        }
    }
}

[DebuggerStepThrough]
internal sealed class AnonymousRSubscriber<T>(Action<T> onNext, Action<Exception> onErrorResume) : Subscriber<T>
{
    [DebuggerStepThrough]
    protected override void OnNextCore(T value)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(result.Exception);
        }
    }
}


[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<T>(Action<T> onNext, Action<Exception> onErrorResume, Action<Result> onComplete) : Subscriber<T>
{
    [DebuggerStepThrough]
    protected override void OnNextCore(T value)
    {
        onNext(message);
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(Result complete)
    {
        onComplete(complete);
    }
}
