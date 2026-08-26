using System.Runtime.CompilerServices;

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
                ThrowObjectDisposedException();
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

    // throws exception when state is disposed
    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                    ThrowObjectDisposedException();
                    break;
            }

            return false; // not here.
        }
    }

    public bool IsDisposed => Volatile.Read(ref completeState) == Disposed;

    public bool IsCompletedOrDisposed
    {
        get
        {
            var state = Volatile.Read(ref completeState);
            return state == Disposed || state == CompletedSuccess || state == CompletedFailure;
        }
    }

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
                ThrowObjectDisposedException();
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

    static void ThrowObjectDisposedException()
    {
        throw new ObjectDisposedException("");
    }
}

public class Subject<T> : Observable<T>, ISubject<T>, IDisposable
{
    // similar implementation to ReactiveProperty

    CompleteState completeState;
    ObserverNode? root;
    ulong version = 1;

    public bool IsDisposed => completeState.IsDisposed;
    internal bool IsCompletedOrDisposed => completeState.IsCompletedOrDisposed;

    public void OnNext(T value)
    {
        if (completeState.IsCompleted) return;

        var currentVersion = GetVersion();
        var node = root;
        while (node != null)
        {
            if (node.Version > currentVersion) break;
            node.Observer.OnNext(value);
            node = node.Next;
        }
    }

    public void OnErrorResume(Exception error)
    {
        if (completeState.IsCompleted) return;

        var currentVersion = GetVersion();
        var node = root;
        while (node != null)
        {
            if (node.Version > currentVersion) break;
            node.Observer.OnErrorResume(error);
            node = node.Next;
        }
    }

    public void OnCompleted(Result result)
    {
        var status = completeState.TrySetResult(result);
        if (status != CompleteState.ResultStatus.Done)
        {
            return; // already completed
        }

        var currentVersion = GetVersion();
        var node = root;
        while (node != null)
        {
            if (node.Version > currentVersion) break;
            node.Observer.OnCompleted(result);
            node = node.Next;
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

        var subscription = new ObserverNode(this, observer, version);

        // need to check called completed during adding
        result = completeState.TryGetResult();
        if (result != null)
        {
            subscription.Observer.OnCompleted(result.Value);
            subscription.Dispose();
            return Disposable.Empty;
        }

        return subscription;
    }

    void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException("");
    }

    public void Dispose()
    {
        Dispose(true);
    }

    public void Dispose(bool callOnCompleted)
    {
        if (completeState.TrySetDisposed(out var alreadyCompleted))
        {
            if (!alreadyCompleted && callOnCompleted)
            {
                var currentVersion = GetVersion();
                var node = root;
                Volatile.Write(ref root, null);
                while (node != null)
                {
                    if (node.Version > currentVersion) break;
                    node.Observer.OnCompleted();
                    node = node.Next;
                }
            }
            else
            {
                Volatile.Write(ref root, null);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ulong GetVersion()
    {
        ulong currentVersion;
        if (version == ulong.MaxValue)
        {
            ResetAllObserverVersion();
            currentVersion = 0;
        }
        else
        {
            currentVersion = version++;
        }
        return currentVersion;

        void ResetAllObserverVersion()
        {
            lock (this)
            {
                var node = root;
                while (node != null)
                {
                    node.Version = 0;
                    node = node.Next;
                }

                version = 1; // also reset version
            }
        }
    }

    sealed class ObserverNode : IDisposable
    {
        public readonly Observer<T> Observer;

        Subject<T>? parent;

        public ObserverNode? Previous { get; set; } // Previous is last node or root(null).
        public ObserverNode? Next { get; set; }
        public ulong Version; // internal use, allow reset

        public ObserverNode(Subject<T> parent, Observer<T> observer, ulong version)
        {
            this.parent = parent;
            this.Observer = observer;
            this.Version = version;

            lock (parent)
            {
                if (parent.root == null)
                {
                    // Single list(both previous and next is null)
                    Volatile.Write(ref parent.root, this);
                }
                else
                {
                    // previous is last, null then root is last.
                    var lastNode = parent.root.Previous ?? parent.root;

                    lastNode.Next = this;
                    this.Previous = lastNode;
                    parent.root.Previous = this;
                }
            }
        }

        public void Dispose()
        {
            var p = Interlocked.Exchange(ref parent, null);
            if (p == null) return;

            // keep this.Next for dispose on iterating
            // Remove node(self) from list
            lock (p)
            {
                if (p.IsCompletedOrDisposed) return;

                if (this == p.root)
                {
                    if (this.Previous == null || this.Next == null)
                    {
                        // case of single list
                        p.root = null;
                    }
                    else
                    {
                        // otherwise, root is next node.
                        var root = this.Next;

                        // single list.
                        if (root.Next == null)
                        {
                            root.Previous = null;
                        }
                        else
                        {
                            root.Previous = this.Previous; // as last.
                        }

                        p.root = root;
                    }
                }
                else
                {
                    // node is not root, previous must exists
                    this.Previous!.Next = this.Next;
                    if (this.Next != null)
                    {
                        this.Next.Previous = this.Previous;
                    }
                    else
                    {
                        // next does not exists, previous is last node so modify root
                        p.root!.Previous = this.Previous;
                    }
                }
            }
        }
    }
}
