using System.Runtime.ExceptionServices;

namespace R3;

public sealed class BehaviorSubject<T> : Observable<T>, ISubject<T>, IDisposable
{
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)

    T latestValue;

    public BehaviorSubject(T initialValue)
    {
        this.list = new FreeListCore<Subscription>(this); // use self as gate(reduce memory usage), this is slightly dangerous so don't lock this in user.
        this.latestValue = initialValue;
    }

    public bool IsDisposed => completeState.IsDisposed;

    public T Value
    {
        get
        {
            var result = completeState.TryGetResult();
            if (result != null && result.Value.IsFailure)
            {
                ExceptionDispatchInfo.Capture(result.Value.Exception).Throw();
            }
            return latestValue;
        }
    }

    public void OnNext(T value)
    {
        if (completeState.IsCompleted) return;

        lock (this)
        {
            this.latestValue = value;
            foreach (var subscription in list.AsSpan())
            {
                subscription?.observer.OnNext(value);
            }
        }
    }

    public void OnErrorResume(Exception error)
    {
        if (completeState.IsCompleted) return;

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnErrorResume(error);
        }
    }

    public void OnCompleted(Result result)
    {
        var status = completeState.TrySetResult(result);
        if (status != CompleteState.ResultStatus.Done)
        {
            return; // already completed
        }

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnCompleted(result);
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        lock (this)
        {
            var result = completeState.TryGetResult();
            if (result != null)
            {
                observer.OnCompleted(result.Value);
                return Disposable.Empty;
            }

            observer.OnNext(latestValue);
            var subscription = new Subscription(this, observer); // create subscription and add observer to list.

            // need to check called completed during adding
            result = completeState.TryGetResult();
            if (result != null)
            {
                subscription.observer.OnCompleted(result.Value);
                subscription.Dispose();
                return Disposable.Empty;
            }

            return subscription;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool callOnCompleted)
    {
        if (completeState.TrySetDisposed(out var alreadyCompleted))
        {
            if (callOnCompleted && !alreadyCompleted)
            {
                // not yet disposed so can call list iteration
                foreach (var subscription in list.AsSpan())
                {
                    subscription?.observer.OnCompleted();
                }
            }

            list.Dispose();
            lock (this)
            {
                this.latestValue = default!;
            }
        }
    }

    sealed class Subscription : IDisposable
    {
        public readonly Observer<T> observer;
        readonly int removeKey;
        BehaviorSubject<T>? parent;

        public Subscription(BehaviorSubject<T> parent, Observer<T> observer)
        {
            this.parent = parent;
            this.observer = observer;
            parent.list.Add(this, out removeKey); // for the thread-safety, add and set removeKey in same lock.
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            // removeKey is index, will reuse if remove completed so only allows to call from here and must not call twice.
            p.list.Remove(removeKey);
        }
    }
}
