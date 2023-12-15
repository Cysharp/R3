#pragma warning disable CS0618

using System.Diagnostics;

namespace R3;

public abstract class Observable<T>
{
    [StackTraceHidden, DebuggerStepThrough]
    public IDisposable Subscribe(Observer<T> subscriber)
    {
        try
        {
            var subscription = SubscribeCore(subscriber);

            if (SubscriptionTracker.TryTrackActiveSubscription(subscription, 2, out var trackableDisposable))
            {
                subscription = trackableDisposable;
            }

            subscriber.SourceSubscription.Disposable = subscription;
            return subscriber; // return subscriber to make subscription chain.
        }
        catch
        {
            subscriber.Dispose(); // when SubscribeCore failed, auto detach caller subscriber
            throw;
        }
    }

    protected abstract IDisposable SubscribeCore(Observer<T> subscriber);
}

public abstract class Observer<T> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in Event<T>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    int calledOnCompleted;
    int disposed;

    public bool IsDisposed => Volatile.Read(ref disposed) != 0;
    bool IsCalledCompleted => Volatile.Read(ref calledOnCompleted) != 0;

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
            EventSystem.GetUnhandledExceptionHandler().Invoke(ex);
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

        try
        {
            OnCompletedCore(result);
        }
        catch (Exception ex)
        {
            EventSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
        finally
        {
            Dispose();
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
