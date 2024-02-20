namespace R3;

public static partial class Observable
{
    public static Observable<T> Merge<T>(params Observable<T>[] sources)
    {
        return new Merge<T>(sources);
    }

    public static Observable<T> Merge<T>(this IEnumerable<Observable<T>> sources)
    {
        return new Merge<T>(sources);
    }
}

public static partial class ObservableExtensions
{
    public static Observable<T> Merge<T>(this Observable<T> source, Observable<T> second)
    {
        return new Merge<T>(new[] { source, second });
    }
}

internal sealed class Merge<T>(IEnumerable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var merge = new _Merge(observer);
        var builder = Disposable.CreateBuilder();

        var count = 0;
        foreach (var item in sources)
        {
            item.Subscribe(new _MergeObserver(merge)).AddTo(ref builder);
            count++;
        }

        merge.disposable.Disposable = builder.Build();

        merge.SetSourceCount(count);

        return merge;
    }

    sealed class _Merge(Observer<T> observer) : IDisposable
    {
        public Observer<T> observer = observer;
        public SingleAssignmentDisposableCore disposable;
        public readonly object gate = new object();

        int sourceCount = -1; // not set yet.
        int completeCount;

        public void SetSourceCount(int count)
        {
            lock (gate)
            {
                sourceCount = count;
                if (sourceCount == completeCount)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        // when all sources are completed, then this observer is completed
        public void TryPublishCompleted()
        {
            lock (gate)
            {
                completeCount++;
                if (completeCount == sourceCount)
                {
                    observer.OnCompleted();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }

    sealed class _MergeObserver(_Merge parent) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            lock (parent.gate)
            {
                parent.observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (parent.gate)
            {
                parent.observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                // when error, publish OnCompleted immediately
                lock (parent.gate)
                {
                    parent.observer.OnCompleted(result);
                }
            }
            else
            {
                parent.TryPublishCompleted();
            }
        }
    }
}
