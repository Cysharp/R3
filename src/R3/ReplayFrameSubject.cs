namespace R3;

public sealed class ReplayFrameSubject<T> : Observable<T>, ISubject<T>, IDisposable
{
    readonly int bufferSize;
    readonly int window;
    readonly FrameProvider frameProvider;
    readonly RingBuffer<(long timestamp, T value)> replayBuffer; // lock object

    // Subject
    FreeListCore<Subscription> list;
    CompleteState completeState;

    public ReplayFrameSubject(int window)
        : this(int.MaxValue, int.MaxValue, ObservableSystem.DefaultFrameProvider)
    {
    }

    public ReplayFrameSubject(int window, FrameProvider frameProvider)
        : this(int.MaxValue, window, frameProvider)
    {
    }

    public ReplayFrameSubject(int bufferSize, int window)
        : this(bufferSize, window, ObservableSystem.DefaultFrameProvider)
    {
    }

    // full constructor
    public ReplayFrameSubject(int bufferSize, int window, FrameProvider frameProvider)
    {
        this.bufferSize = bufferSize;
        this.window = window;
        this.frameProvider = frameProvider;
        this.replayBuffer = new RingBuffer<(long, T)>(bufferSize < 8 ? bufferSize : 8);
        this.list = new FreeListCore<Subscription>(replayBuffer);
    }

    public bool IsDisposed => completeState.IsDisposed;

    public void OnNext(T value)
    {
        if (completeState.IsCompleted) return;

        lock (replayBuffer)
        {
            Trim();
            replayBuffer.AddLast((frameProvider?.GetFrameCount() ?? 0, value));
        }

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
        // raise latest value on subscribe(before check completed add observer to list)
        lock (replayBuffer)
        {
            Trim(); // Trim before get span
            var dualSpan = replayBuffer.GetSpan();
            foreach (ref readonly var item in dualSpan.First)
            {
                observer.OnNext(item.value);
            }
            foreach (ref readonly var item in dualSpan.Second)
            {
                observer.OnNext(item.value);
            }
        }

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
            lock (replayBuffer)
            {
                replayBuffer.Clear();
            }
        }
    }

    void Trim()
    {
        // Trim by Count
        while (replayBuffer.Count > bufferSize)
        {
            replayBuffer.RemoveFirst();
        }

        // Trim by Time
        var now = frameProvider.GetFrameCount();
        while (replayBuffer.Count > 0)
        {
            var value = replayBuffer[0]; // peek first
            var elapsed = now - value.timestamp;
            if (elapsed >= window)
            {
                replayBuffer.RemoveFirst();
            }
            else
            {
                break;
            }
        }
    }

    sealed class Subscription : IDisposable
    {
        public readonly Observer<T> observer;
        readonly int removeKey;
        ReplayFrameSubject<T>? parent;

        public Subscription(ReplayFrameSubject<T> parent, Observer<T> observer)
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
