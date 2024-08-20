namespace R3;

public sealed class SingleAssignmentSubject<T> : Observable<T>, ISubject<T>, IDisposable
{
    Observer<T>? singleObserver;
    Result completed;

    public bool IsDisposed => singleObserver == DisposedObserver.Instance;

    public void OnNext(T value)
    {
        var observer = singleObserver;
        if (observer == CompletedObserver.Instance || observer == null)
        {
            // do nothing
        }
        else if (observer == DisposedObserver.Instance)
        {
            ThrowAlreadyDisposed();
        }
        else
        {
            observer.OnNext(value);
        }
    }

    public void OnErrorResume(Exception error)
    {
        var observer = singleObserver;
        if (observer == CompletedObserver.Instance || observer == null)
        {
            // do nothing
        }
        else if (observer == DisposedObserver.Instance)
        {
            ThrowAlreadyDisposed();
        }
        else
        {
            observer.OnErrorResume(error);
        }
    }

    public void OnCompleted(Result complete)
    {
        while (true)
        {
            var observer = Volatile.Read(ref singleObserver);
            if (observer == CompletedObserver.Instance)
            {
                // do nothing
                return;
            }
            else if (observer == DisposedObserver.Instance)
            {
                ThrowAlreadyDisposed();
                return;
            }
            else
            {
                this.completed = complete;
                if (Interlocked.CompareExchange(ref singleObserver, CompletedObserver.Instance, observer) == observer)
                {
                    observer?.OnCompleted(complete);
                    return;
                }
            }
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var field = Interlocked.CompareExchange(ref singleObserver, observer, null);
        if (field == null)
        {
            // ok to set.
            return new Subscription(this);
        }

        if (field == DisposedObserver.Instance)
        {
            ThrowAlreadyDisposed();
        }
        else if (field == CompletedObserver.Instance)
        {
            observer.OnCompleted(completed);
        }
        else
        {
            ThrowAlreadyAssignment();
        }
        return Disposable.Empty;
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool callOnCompleted)
    {
        var observer = Interlocked.Exchange(ref singleObserver, DisposedObserver.Instance);
        if (observer != DisposedObserver.Instance && observer != null && callOnCompleted)
        {
            observer.OnCompleted();
        }
    }

    static void ThrowAlreadyAssignment()
    {
        throw new InvalidOperationException("Observer is already assigned.");
    }

    void ThrowAlreadyDisposed()
    {
        throw new ObjectDisposedException("");
    }

    class Subscription(SingleAssignmentSubject<T> parent) : IDisposable
    {
        public void Dispose()
        {
            while (true)
            {
                var observer = Volatile.Read(ref parent.singleObserver);
                if (observer == CompletedObserver.Instance || observer == DisposedObserver.Instance || observer == null)
                {
                    // do nothing
                    return;
                }
                else
                {
                    // reset to null(allow multiple assignment after first subscription is disposed)
                    if (Interlocked.CompareExchange(ref parent.singleObserver, null, observer) == observer)
                    {
                        return;
                    }
                }
            }
        }
    }

    sealed class CompletedObserver : Observer<T>
    {
        public static readonly CompletedObserver Instance = new();

        protected override void OnCompletedCore(Result result)
        {
        }

        protected override void OnErrorResumeCore(Exception error)
        {
        }

        protected override void OnNextCore(T value)
        {
        }
    }

    sealed class DisposedObserver : Observer<T>
    {
        public static readonly DisposedObserver Instance = new();

        protected override void OnCompletedCore(Result result)
        {
        }

        protected override void OnErrorResumeCore(Exception error)
        {
        }

        protected override void OnNextCore(T value)
        {
        }
    }
}
