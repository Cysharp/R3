namespace R3;

public static partial class Observable
{
    public static Observable<T> Merge<T>(Observable<Observable<T>> sources)
    {
        return new MergeOuter<T>(sources);
    }
}

internal sealed class MergeOuter<T>(Observable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return sources.Subscribe(new _MergeOuter(observer));
    }

    sealed class _MergeOuter(Observer<T> observer) : Observer<Observable<T>>
    {
        // keep when inner is running
        protected override bool AutoDisposeOnCompleted => false;

        public readonly Observer<T> observer = observer;
        public readonly object gate = new();
        public readonly CompositeDisposable subscriptions = new();
        public bool stopped;

        protected override void OnNextCore(Observable<T> value)
        {
            var innerObserver = new _MergeInner(this);
            var subscription = value.Subscribe(innerObserver);

            lock (gate)
            {
                subscriptions.Add(subscription);
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
            lock (gate)
            {
                if (result.IsFailure)
                {
                    try
                    {
                        observer.OnCompleted(result);
                    }
                    finally
                    {
                        Dispose();
                    }
                    return;
                }

                stopped = true;
                // when no running inner
                if (subscriptions.Count <= 0)
                {
                    try
                    {
                        observer.OnCompleted(result);
                    }
                    finally
                    {
                        Dispose();
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            subscriptions.Dispose();
        }
    }

    sealed class _MergeInner(_MergeOuter outer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            lock (outer.gate)
            {
                outer.observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (outer.gate)
            {
                outer.observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (outer.gate)
            {
                // this(Observer) and SourceSubscription handled by outside are the same. So Remove(this) works.
                // this inner is disposed of by SourceSubscription when Dispose.
                outer.subscriptions.Remove(this);

                if (result.IsFailure)
                {
                    outer.observer.OnCompleted(result);
                }
                else
                {
                    // when all sources are completed, then this observer is completed
                    if (outer is { stopped: true, subscriptions.Count: <= 0 })
                    {
                        outer.observer.OnCompleted();
                    }
                }
            }
        }
    }
}
