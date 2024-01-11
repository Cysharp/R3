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
        return new _MergeOuter(sources, observer).Run();
    }

    sealed class _MergeOuter(Observable<Observable<T>> sources, Observer<T> observer) : Observer<Observable<T>>
    {
        // keep when inner is running
        protected override bool AutoDisposeOnCompleted => false;

        public readonly Observer<T> observer = observer;
        public readonly object gate = new();
        public readonly CompositeDisposable subscriptions = new();

        IDisposable? collectionSourceSubscription;

        public IDisposable Run()
        {
            collectionSourceSubscription = sources.Subscribe(this);
            subscriptions.Add(collectionSourceSubscription);
            return subscriptions;
        }

        protected override void OnNextCore(Observable<T> value)
        {
            var subscription = new SingleAssignmentDisposableCore();
            var innerObserver = new _MergeInner(this, subscription);
            subscription.Disposable = value.Subscribe(innerObserver);

            lock (gate)
            {
                subscriptions.Add(subscription.Disposable);
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

                // when no running inners any
                if (subscriptions.Count <= 1)
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
    }

    sealed class _MergeInner(_MergeOuter outer, SingleAssignmentDisposableCore subscriptionSelf) : Observer<T>
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
                // this inner is disposed of by SourceSubscription on the outside.
                outer.subscriptions.Remove(subscriptionSelf.Disposable!);

                if (result.IsFailure)
                {
                    outer.observer.OnCompleted(result);
                }
                else
                {
                    // when all sources are completed, then this observer is completed
                    if (outer.subscriptions.Count <= 1)
                    {
                        outer.observer.OnCompleted();
                    }
                }
            }
        }
    }
}
