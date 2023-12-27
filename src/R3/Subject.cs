using System.Runtime.CompilerServices;

namespace R3;

public sealed class Subject<T> : Observable<T>, ISubject<T>, IDisposable
{
    int calledCompleted = 0;
    Result completeValue;
    FreeListCore<_CompletablePublisher> list;
    readonly object completedLock = new object();

    public Subject()
    {
        list = new FreeListCore<_CompletablePublisher>(this);
    }

    public void OnNext(T value)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        foreach (var observer in list.AsSpan())
        {
            if (observer != null)
            {
                observer.OnNext(value);
            }
        }
    }

    public void OnErrorResume(Exception error)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        foreach (var observer in list.AsSpan())
        {
            if (observer != null)
            {
                observer.OnErrorResume(error);
            }
        }
    }

    public void OnCompleted(Result complete)
    {
        if (list.IsDisposed) ThrowDisposed();
        if (Volatile.Read(ref calledCompleted) != 0) return;

        // need lock for Subscribe after OnCompleted
        lock (completedLock)
        {
            completeValue = complete;
            calledCompleted = 1;
        }

        foreach (var observer in list.AsSpan())
        {
            if (observer != null)
            {
                observer.OnCompleted(complete);
            }
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (list.IsDisposed) ThrowDisposed();

        lock (completedLock)
        {
            if (Volatile.Read(ref calledCompleted) != 0)
            {
                observer.OnCompleted(completeValue);
                return Disposable.Empty;
            }

            // need lock after Add
            var subscription = new _CompletablePublisher(this, observer);
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
        // TODO: when dispose, call OnCompleted to dispose all observers.

        list.Dispose();
    }

    static void ThrowDisposed()
    {
        throw new ObjectDisposedException("CompletablePublisher");
    }

    sealed class _CompletablePublisher(Subject<T>? parent, Observer<T> observer) : IDisposable
    {
        public int removeKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(T value)
        {
            observer.OnNext(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnErrorResume(Exception error)
        {
            observer.OnErrorResume(error);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted(Result complete)
        {
            observer.OnCompleted(complete);
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            p.Unsubscribe(this);
        }
    }
}
