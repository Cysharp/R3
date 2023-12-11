#pragma warning disable CS0618

using System.Diagnostics;

namespace R3;

// similar as IObservable<T> 
// IDisposable Subscribe(Subscriber<TMessage> subscriber)
public abstract class Event<TMessage>
{
    [StackTraceHidden, DebuggerStepThrough]
    public IDisposable Subscribe(Subscriber<TMessage> subscriber)
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

    [StackTraceHidden, DebuggerStepThrough]
    protected abstract IDisposable SubscribeCore(Subscriber<TMessage> subscriber);
}

// similar as IObserver<T> but no stop on OnError.
public abstract class Subscriber<TMessage> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in Event<TMessage>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    int calledDispose;

    public bool IsDisposed => Volatile.Read(ref calledDispose) != 0;

    [StackTraceHidden, DebuggerStepThrough]
    public void OnNext(TMessage message)
    {
        if (IsDisposed) return;
        try
        {
            OnNextCore(message);
        }
        catch (Exception ex)
        {
            OnErrorResume(ex);
        }
    }

    protected abstract void OnNextCore(TMessage message);

    [StackTraceHidden, DebuggerStepThrough]
    public void OnErrorResume(Exception error)
    {
        if (IsDisposed) return;
        OnErrorResumeCore(error);
    }

    protected abstract void OnErrorResumeCore(Exception error);

    [StackTraceHidden, DebuggerStepThrough]
    public void Dispose()
    {
        if (Interlocked.Exchange(ref calledDispose, 1) != 0)
        {
            return;
        }

        DisposeCore();                // Dispose self
        SourceSubscription.Dispose(); // Dispose attached parent
    }

    [StackTraceHidden, DebuggerStepThrough]
    protected virtual void DisposeCore() { }
}

// similar as IObservable<T>
public abstract class CompletableEvent<TMessage, TComplete>
{
    [StackTraceHidden, DebuggerStepThrough]
    public IDisposable Subscribe(Subscriber<TMessage, TComplete> subscriber)
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

    protected abstract IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber);
}

// similar as IObserver<T>
public abstract class Subscriber<TMessage, TComplete> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in CompletableEvent<TMessage>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    int calledOnCompleted;
    int disposed;

    public bool IsDisposed => Volatile.Read(ref disposed) != 0;
    bool IsCalledCompleted => Volatile.Read(ref calledOnCompleted) != 0;

    [StackTraceHidden, DebuggerStepThrough]
    public void OnNext(TMessage message)
    {
        if (IsDisposed || IsCalledCompleted) return;

        try
        {
            OnNextCore(message);
        }
        catch (Exception ex)
        {
            OnErrorResume(ex);
        }
    }

    protected abstract void OnNextCore(TMessage message);

    [StackTraceHidden, DebuggerStepThrough]
    public void OnErrorResume(Exception error)
    {
        if (IsDisposed || IsCalledCompleted) return;

        OnErrorResumeCore(error);
    }

    protected abstract void OnErrorResumeCore(Exception error);

    [StackTraceHidden, DebuggerStepThrough]
    public void OnCompleted(TComplete complete)
    {
        if (Interlocked.Exchange(ref calledOnCompleted, 1) != 0)
        {
            return;
        }
        if (IsDisposed) return;

        try
        {
            OnCompletedCore(complete);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            Dispose();
        }
    }

    protected abstract void OnCompletedCore(TComplete complete);

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
