namespace R3;

public static partial class ObservableExtensions
{
    public static Task<bool> SequenceEqualAsync<T>(this Observable<T> source, Observable<T> second, CancellationToken cancellationToken = default)
    {
        return SequenceEqualAsync(source, second, EqualityComparer<T>.Default, cancellationToken);
    }

    public static Task<bool> SequenceEqualAsync<T>(this Observable<T> source, Observable<T> second, IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken = default)
    {
        var method = new SequenceEqualAsync<T>(equalityComparer, cancellationToken);
        try
        {
            source.Subscribe(method.leftObserver);
            second.Subscribe(method.rightObserver);
        }
        catch
        {
            method.Dispose();
            throw;
        }

        return method.Task;
    }
}

// SequenceEqualAsync
internal sealed class SequenceEqualAsync<T> : TaskObserverBase<T, bool>
{
    public readonly IEqualityComparer<T> equalityComparer;
    public SequenceEqualAsyncObserver leftObserver;
    public SequenceEqualAsyncObserver rightObserver;

    public SequenceEqualAsync(IEqualityComparer<T> equalityComparer, CancellationToken cancellationToken)
        : base(cancellationToken)
    {
        this.equalityComparer = equalityComparer;
        this.leftObserver = new SequenceEqualAsyncObserver(this);
        this.rightObserver = new SequenceEqualAsyncObserver(this);
    }

    protected override void OnNextCore(T value)
    {
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
        }
    }

    protected override void DisposeCore()
    {
        leftObserver.Dispose();
        rightObserver.Dispose();
    }

    // called in lock
    void CheckValues()
    {
        while (leftObserver.values.Count != 0 && rightObserver.values.Count != 0)
        {
            var left = leftObserver.values.Dequeue();
            var right = rightObserver.values.Dequeue();
            if (!equalityComparer.Equals(left, right))
            {
                TrySetResult(false);
                return;
            }
        }

        if (leftObserver.IsCompleted && rightObserver.IsCompleted)
        {
            if (leftObserver.values.Count == 0 && rightObserver.values.Count == 0)
            {
                TrySetResult(true);
            }
            else
            {
                TrySetResult(false);
            }
        }
    }

    internal sealed class SequenceEqualAsyncObserver(SequenceEqualAsync<T> parent) : Observer<T>
    {
        public Queue<T> values = new Queue<T>();
        public bool IsCompleted;

        protected override void OnNextCore(T value)
        {
            lock (parent)
            {
                values.Enqueue(value);
                parent.CheckValues();
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            parent.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                parent.OnCompleted(result);
            }
            else
            {
                lock (parent)
                {
                    IsCompleted = true;
                    parent.CheckValues();
                }
            }
        }
    }
}
