using System.Runtime.CompilerServices;

namespace R3;

public interface IEventPublisher<T>
{
    void PublishOnNext(T value);
    void PublishOnCompleted(Result complete);
}

public sealed class Publisher<T> : Observable<T>, IEventPublisher<T>, IDisposable
{
    int calledCompleted = 0;
    Result completeValue;
    FreeListCore<_CompletablePublisher> list;
    readonly object completedLock = new object();

    public Publisher()
    {
        list = new FreeListCore<_CompletablePublisher>(this);
    }

    public void PublishOnNext(T value)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnNext(value);
            }
        }
    }

    public void PublishOnErrorResume(Exception error)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnErrorResume(error);
            }
        }
    }

    public void PublishOnCompleted(Result complete)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        // need lock for Subscribe after OnCompleted
        lock (completedLock)
        {
            completeValue = complete;
            calledCompleted = 1;
        }

        foreach (var subscriber in list.AsSpan())
        {
            if (subscriber != null)
            {
                subscriber.OnCompleted(complete);
            }
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        if (list.IsDisposed) ThrowDisposed();

        lock (completedLock)
        {
            if (Volatile.Read(ref calledCompleted) != 0)
            {
                subscriber.OnCompleted(completeValue);
                return Disposable.Empty;
            }

            // need lock after Add
            var subscription = new _CompletablePublisher(this, subscriber);
            subscription.removeKey = list.Add(subscription); // when disposed, may throw DisposedException in this line
            return subscription;
        }
    }

    void Unsubscribe(_CompletablePublisher subscription)
    {
        list.Remove(subscription.removeKey);
    }

    public void Dispose()
    {
        // TODO: when dispose, call OnCompleted to dispose all subscribers.

        list.Dispose();
    }

    static void ThrowDisposed()
    {
        throw new ObjectDisposedException("CompletablePublisher");
    }

    sealed class _CompletablePublisher(Publisher<T>? parent, Observer<T> subscriber) : IDisposable
    {
        public int removeKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value)
        {
            subscriber.OnNext(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnErrorResume(Exception error)
        {
            subscriber.OnErrorResume(error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Result complete)
        {
            subscriber.OnCompleted(complete);
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            p.Unsubscribe(this);
        }
    }
}
