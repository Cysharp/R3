using System.Diagnostics;

namespace R3;

public static class EventSubscribeExtensions
{
    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source)
    {
        return source.Subscribe(NopSubscriber<T>.Instance);
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T>(this Event<T> source, Action<T> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<T>(onNext, EventSystem.GetUnhandledExceptionHandler(), Stubs.HandleResult));
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

    // with state

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T, TState>(this Event<T> source, TState state, Action<T, TState> onNext)
    {
        return source.Subscribe(new AnonymousSubscriber<T, TState>(onNext, Stubs<TState>.HandleException, Stubs<TState>.HandleResult, state));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T, TState>(this Event<T> source, TState state, Action<T, TState> onNext, Action<Result, TState> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<T, TState>(onNext, Stubs<TState>.HandleException, onComplete, state));
    }

    [DebuggerStepThrough]
    public static IDisposable Subscribe<T, TState>(this Event<T> source, TState state, Action<T, TState> onNext, Action<Exception, TState> onErrorResume, Action<Result, TState> onComplete)
    {
        return source.Subscribe(new AnonymousSubscriber<T, TState>(onNext, onErrorResume, onComplete, state));
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
        onNext(value);
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
        onNext(value);
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

[DebuggerStepThrough]
internal sealed class AnonymousSubscriber<T, TState>(Action<T, TState> onNext, Action<Exception, TState> onErrorResume, Action<Result, TState> onComplete, TState state) : Subscriber<T>
{
    [DebuggerStepThrough]
    protected override void OnNextCore(T value)
    {
        onNext(value, state);
    }

    [DebuggerStepThrough]
    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error, state);
    }

    [DebuggerStepThrough]
    protected override void OnCompletedCore(Result complete)
    {
        onComplete(complete, state);
    }
}
