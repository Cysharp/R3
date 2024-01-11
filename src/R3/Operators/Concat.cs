namespace R3;

public static partial class Observable
{
    public static Observable<T> Concat<T>(this Observable<Observable<T>> sources)
    {
        return new ConcatMany<T>(sources);
    }
}

internal sealed class ConcatMany<T>(Observable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return sources.Subscribe(new _ConcatMany(observer));
    }

    sealed class _ConcatMany(Observer<T> observer) : Observer<Observable<T>>
    {
        readonly Observer<T> observer = observer;
        readonly object gate = new();
        readonly Queue<Observable<T>> q = new();

        SerialDisposableCore serialDisposable;
        bool isStopped;
        int activeCount;

        // keep when inner is running
        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(Observable<T> value)
        {
            lock (gate)
            {
                if (activeCount < 1)
                {
                    activeCount++;
                    serialDisposable.Disposable = value.Subscribe(new ConcatInner(this));
                }
                else
                {
                    q.Enqueue(value);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                PublishCompleted(result);
            }
            else
            {
                lock (gate)
                {
                    isStopped = true;
                    if (activeCount == 0)
                    {
                        PublishCompleted(result);
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            serialDisposable.Dispose();
        }

        void PublishCompleted(Result result)
        {
            try
            {
                lock (gate)
                {
                    observer.OnCompleted(result);
                }
            }
            finally
            {
                Dispose();
            }
        }

        sealed class ConcatInner(_ConcatMany parent) : Observer<T>
        {
            // Manual disposing by SerialDisposableCore
            protected override bool AutoDisposeOnCompleted => false;

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
                    parent.OnCompleted();
                }
                else
                {
                    lock (parent.gate)
                    {
                        if (parent.q.Count > 0)
                        {
                            var nextSource = parent.q.Dequeue();
                            parent.serialDisposable.Disposable = nextSource.Subscribe(new ConcatInner(parent));
                        }
                        else
                        {
                            parent.activeCount--;
                            if (parent is { isStopped: true, activeCount: 0 })
                            {
                                parent.PublishCompleted(result);
                            }
                        }
                    }
                }
            }
        }
    }
}
