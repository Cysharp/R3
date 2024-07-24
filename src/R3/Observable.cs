#pragma warning disable CS0618

using System.Diagnostics;

namespace R3;

public abstract class Observable<T>
{
    [StackTraceHidden, DebuggerStepThrough]
    public IDisposable Subscribe(Observer<T> observer)
    {
        try
        {
            var subscription = SubscribeCore(observer);

            if (ObservableTracker.TryTrackActiveSubscription(subscription, 2, out var trackableDisposable))
            {
                subscription = trackableDisposable;
            }

            observer.SourceSubscription.Disposable = subscription;
            return observer; // return observer to make subscription chain.
        }
        catch
        {
            observer.Dispose(); // when SubscribeCore failed, auto detach caller observer
            throw;
        }
    }

    protected abstract IDisposable SubscribeCore(Observer<T> observer);
}

public abstract class Observer<T> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in Observable<T>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    int calledOnCompleted;
    int disposed;

    public bool IsDisposed => disposed != 0;
    bool IsCalledCompleted => calledOnCompleted != 0;

    // enable/disable auto dispose on completed.
    protected virtual bool AutoDisposeOnCompleted => true;

    [StackTraceHidden, DebuggerStepThrough]
    public void OnNext(T value)
    {
        if (IsDisposed || IsCalledCompleted) return;

        try
        {
            OnNextCore(value);
        }
        catch (Exception ex)
        {
            OnErrorResume(ex);
        }
    }

    protected abstract void OnNextCore(T value);

    [StackTraceHidden, DebuggerStepThrough]
    public void OnErrorResume(Exception error)
    {
        if (IsDisposed || IsCalledCompleted) return;

        try
        {
            OnErrorResumeCore(error);
        }
        catch (Exception ex)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
    }

    protected abstract void OnErrorResumeCore(Exception error);

    [StackTraceHidden, DebuggerStepThrough]
    public void OnCompleted(Result result)
    {
        if (Interlocked.Exchange(ref calledOnCompleted, 1) != 0)
        {
            return;
        }
        if (IsDisposed) return;

        var disposeOnFinally = AutoDisposeOnCompleted;
        try
        {
            OnCompletedCore(result);
        }
        catch (Exception ex)
        {
            disposeOnFinally = true;
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
        finally
        {
            if (disposeOnFinally)
            {
                Dispose();
            }
        }
    }

    protected abstract void OnCompletedCore(Result result);

    [StackTraceHidden, DebuggerStepThrough]
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        DisposeCore();                // Dispose self
        SourceSubscription.Dispose(); // Dispose attached parent
    }

    [StackTraceHidden, DebuggerStepThrough]
    protected virtual void DisposeCore() { }
}
