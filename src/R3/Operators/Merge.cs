namespace R3;

public static partial class Observable
{
    public static Observable<T> Merge<T>(this Observable<Observable<T>> sources)
    {
        return new MergeMany<T>(sources);
    }
}

internal sealed class MergeMany<T>(Observable<Observable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return sources.Subscribe(new _MergeMany(observer));
    }

    sealed class _MergeMany(Observer<T> observer) : Observer<Observable<T>>
    {
        // keep when inner is running
        protected override bool AutoDisposeOnCompleted => false;

        readonly Observer<T> observer = observer;
        readonly object gate = new();
        readonly CompositeDisposable subscriptions = new();
        bool isStopped;

        protected override void OnNextCore(Observable<T> value)
        {
            var innerObserver = new MergeInner(this);
            lock (gate)
            {
                // add observer before subscribe
                subscriptions.Add(innerObserver);
            }
            value.Subscribe(innerObserver);
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
                    PublishCompleted(result);
                }
                else
                {
                    isStopped = true;
                    // when no running inner
                    if (subscriptions.Count <= 0)
                    {
                        PublishCompleted(result);
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            subscriptions.Dispose();
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

        sealed class MergeInner(_MergeMany parent) : Observer<T>
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
                lock (parent.gate)
                {
                    if (result.IsFailure)
                    {
                        parent.observer.OnCompleted(result);
                    }
                    else
                    {
                        // when all sources are completed, then this observer is completed
                        if (parent is { isStopped: true, subscriptions.Count: 1 }) // only self
                        {
                            parent.PublishCompleted(result);
                        }
                    }
                }
            }

            protected override void DisposeCore()
            {
                parent.subscriptions.Remove(this);
            }
        }
    }
}
