namespace R3;

// thread-safety state for Subject.
internal struct CompleteState
{
    internal enum ResultStatus
    {
        Done,
        AlreadySuccess,
        AlreadyFailed
    }

    const int NotCompleted = 0;
    const int CompletedSuccess = 1;
    const int CompletedFailure = 2;
    const int Disposed = 3;

    int completeState;
    Exception? error;

    public ResultStatus TrySetResult(Result result)
    {
        int field;
        if (result.IsSuccess)
        {
            field = Interlocked.CompareExchange(ref completeState, CompletedSuccess, NotCompleted); // try set success
        }
        else
        {
            field = Interlocked.CompareExchange(ref completeState, CompletedFailure, NotCompleted); // try set failure
            Volatile.Write(ref error, result.Exception);      // set failure immmediately(but not locked).
        }

        switch (field)
        {
            case NotCompleted:
                return ResultStatus.Done;
            case CompletedSuccess:
                return ResultStatus.AlreadySuccess;
            case CompletedFailure:
                return ResultStatus.AlreadyFailed;
            case Disposed:
                ThrowObjectDiposedException();
                break;
        }

        return ResultStatus.Done; // not here.
    }

    public bool TrySetDisposed(out bool alreadyCompleted)
    {
        var field = Interlocked.Exchange(ref completeState, Disposed);
        switch (field)
        {
            case NotCompleted:
                alreadyCompleted = false;
                return true;
            case CompletedSuccess:
            case CompletedFailure:
                alreadyCompleted = true;
                return true;
            case Disposed:
                break;
        }

        alreadyCompleted = false;
        return false;
    }

    public bool IsCompleted
    {
        get
        {
            switch (completeState)
            {
                case NotCompleted:
                    return false;
                case CompletedSuccess:
                    return true;
                case CompletedFailure:
                    return true;
                case Disposed:
                    ThrowObjectDiposedException();
                    break;
            }

            return false; // not here.
        }
    }

    public bool IsDisposed => Volatile.Read(ref completeState) == Disposed;

    public Result? TryGetResult()
    {
        var currentState = Volatile.Read(ref completeState);

        switch (currentState)
        {
            case NotCompleted:
                return null;
            case CompletedSuccess:
                return Result.Success;
            case CompletedFailure:
                return Result.Failure(GetException());
            case Disposed:
                ThrowObjectDiposedException();
                break;
        }

        return null; // not here.
    }

    // be careful to use, this method need to call after ResultStatus.AlreadyFailed.
    Exception GetException()
    {
        Exception? error = Volatile.Read(ref this.error);
        if (error != null) return error;

        var spinner = new SpinWait();
        do
        {
            spinner.SpinOnce();
            error = Volatile.Read(ref this.error);
        } while (error == null);

        return error;
    }

    static void ThrowObjectDiposedException()
    {
        throw new ObjectDisposedException("");
    }
}

public sealed class Subject<T> : Observable<T>, ISubject<T>, IDisposable
{
    FreeListCore<Subscription> list; // struct(array, int)
    CompleteState completeState;     // struct(int, IntPtr)

    public Subject()
    {
        list = new FreeListCore<Subscription>(this); // use self as gate(reduce memory usage), this is slightly dangerous so don't lock this in user.
    }

    public bool IsDisposed => completeState.IsDisposed;

    public void OnNext(T value)
    {
        if (completeState.IsCompleted) return;

        foreach (var subscription in list.AsSpan())
        {
            subscription?.observer.OnNext(value);
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
        var result = completeState.TryGetResult();
        if (result != null)
        {
            observer.OnCompleted(result.Value);
            return Disposable.Empty;
        }

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
        }
    }

    sealed class Subscription : IDisposable
    {
        public readonly Observer<T> observer;
        readonly int removeKey;
        Subject<T>? parent;

        public Subscription(Subject<T> parent, Observer<T> observer)
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
