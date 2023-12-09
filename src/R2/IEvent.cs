#pragma warning disable CS0618

using System.Diagnostics;

namespace R2;

// similar as IObservable<T> (only OnNext)
// IDisposable Subscribe(Subscriber<TMessage> subscriber)
public abstract class Event<TMessage>
{
    [DebuggerStepThrough]
    public IDisposable Subscribe(Subscriber<TMessage> subscriber)
    {
        try
        {
            // TODO: track subscription
            var subscription = SubscribeCore(subscriber);
            subscriber.SourceSubscription.Disposable = subscription;
            return subscription;
        }
        catch
        {
            subscriber.Dispose(); // when SubscribeCore failed, auto detach caller subscriber
            throw;
        }
    }

    protected abstract IDisposable SubscribeCore(Subscriber<TMessage> subscriber);
}

// similar as IObserver<T>
// void OnNext(TMessage message);
public abstract class Subscriber<TMessage> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in Event<TMessage>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    public bool IsDisposed => SourceSubscription.IsDisposed;

    public abstract void OnNext(TMessage message);
    protected virtual void DisposeCore() { }

    [DebuggerStepThrough]
    public void Dispose()
    {
        if (!SourceSubscription.IsDisposed)
        {
            SourceSubscription.Dispose();
            DisposeCore();
        }
    }
}

// similar as IObservable<T>
public abstract class CompletableEvent<TMessage, TComplete>
{
    [DebuggerStepThrough]
    public IDisposable Subscribe(Subscriber<TMessage, TComplete> subscriber)
    {
        try
        {
            var subscription = SubscribeCore(subscriber);
            subscriber.SourceSubscription.Disposable = subscription;
            return subscription;
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
// void OnNext(TMessage message);
// void OnCompleted(TComplete complete);
public abstract class Subscriber<TMessage, TComplete> : IDisposable
{
#if DEBUG
    [Obsolete("Only allow in CompletableEvent<TMessage>.")]
#endif
    internal SingleAssignmentDisposableCore SourceSubscription;

    public abstract void OnNext(TMessage message);
    [DebuggerStepThrough]
    public void OnCompleted(TComplete complete)
    {
        try
        {
            OnCompletedCore(complete);
        }
        finally
        {
            Dispose();
        }
    }

    protected abstract void OnCompletedCore(TComplete complete);
    protected virtual void DisposeCore() { }

    [DebuggerStepThrough]
    public void Dispose()
    {
        if (!SourceSubscription.IsDisposed)
        {
            SourceSubscription.Dispose();
            DisposeCore();
        }
    }
}
