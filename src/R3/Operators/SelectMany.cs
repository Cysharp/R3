namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> SelectMany<TSource, TResult>(this Observable<TSource> source, Func<TSource, Observable<TResult>> selector)
    {
        return SelectMany(source, selector, static (sourceValue, collectionValue) => collectionValue);
    }


    public static Observable<TResult> SelectMany<TSource, TCollection, TResult>(this Observable<TSource> source, Func<TSource, Observable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        return new SelectMany<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
    }

    // with index

    public static Observable<TResult> SelectMany<TSource, TResult>(this Observable<TSource> source, Func<TSource, int, Observable<TResult>> selector)
    {
        return SelectMany(source, selector, static (sourceValue, sourceIndex, collectionValue, collectionIndex) => collectionValue);
    }

    public static Observable<TResult> SelectMany<TSource, TCollection, TResult>(this Observable<TSource> source, Func<TSource, int, Observable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
    {
        return new SelectManyIndexed<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
    }
}

internal sealed class SelectMany<TSource, TCollection, TResult>(Observable<TSource> source, Func<TSource, Observable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _SelectMany(observer, collectionSelector, resultSelector));
    }

    sealed class _SelectMany(Observer<TResult> observer, Func<TSource, Observable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) : Observer<TSource>
    {
        readonly Observer<TResult> observer = observer;
        readonly Func<TSource, Observable<TCollection>> collectionSelector = collectionSelector;
        readonly Func<TSource, TCollection, TResult> resultSelector = resultSelector;
        readonly CompositeDisposable compositeDisposable = new();
        readonly object gate = new object();
        bool isStopped;

        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(TSource value)
        {
            var nextSource = collectionSelector(value);

            var observer = new _SelectManyCollectionObserver(value, this);
            compositeDisposable.Add(observer); // Add observer before subscribe!
            nextSource.Subscribe(observer);
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
                    if (compositeDisposable.Count == 0)
                    {
                        PublishCompleted(result);
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            compositeDisposable.Dispose();
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

        sealed class _SelectManyCollectionObserver(TSource sourceValue, _SelectMany parent) : Observer<TCollection>
        {
            protected override void OnNextCore(TCollection value)
            {
                var result = parent.resultSelector(sourceValue, value);
                lock (parent.gate)
                {
                    parent.observer.OnNext(result);
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
                    parent.OnCompleted(result);
                }
                else
                {
                    lock (parent.gate)
                    {
                        if (parent.isStopped && parent.compositeDisposable.Count == 1) // only self
                        {
                            parent.PublishCompleted(result);
                        }
                    }
                }
            }

            protected override void DisposeCore()
            {
                parent.compositeDisposable.Remove(this);
            }
        }
    }
}


internal sealed class SelectManyIndexed<TSource, TCollection, TResult>(Observable<TSource> source, Func<TSource, int, Observable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
    : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _SelectMany(observer, collectionSelector, resultSelector));
    }

    sealed class _SelectMany(Observer<TResult> observer, Func<TSource, int, Observable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector) : Observer<TSource>
    {
        readonly Observer<TResult> observer = observer;
        readonly Func<TSource, int, Observable<TCollection>> collectionSelector = collectionSelector;
        readonly Func<TSource, int, TCollection, int, TResult> resultSelector = resultSelector;
        readonly CompositeDisposable compositeDisposable = new();
        readonly object gate = new object();
        bool isStopped;
        int index = 0;

        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(TSource value)
        {
            var i = index++;
            var nextSource = collectionSelector(value, i);
            var observer = new _SelectManyCollectionObserver(value, this, i);
            compositeDisposable.Add(observer); // Add observer before subscribe!
            nextSource.Subscribe(observer);
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
                    if (compositeDisposable.Count == 0)
                    {
                        PublishCompleted(result);
                    }
                }
            }
        }

        protected override void DisposeCore()
        {
            compositeDisposable.Dispose();
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

        sealed class _SelectManyCollectionObserver(TSource sourceValue, _SelectMany parent, int sourceIndex) : Observer<TCollection>
        {
            int index = 0;

            protected override void OnNextCore(TCollection value)
            {
                var result = parent.resultSelector(sourceValue, sourceIndex, value, index++);
                lock (parent.gate)
                {
                    parent.observer.OnNext(result);
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
                    parent.OnCompleted(result);
                }
                else
                {
                    lock (parent.gate)
                    {
                        if (parent.isStopped && parent.compositeDisposable.Count == 1) // only self
                        {
                            parent.PublishCompleted(result);
                        }
                    }
                }
            }

            protected override void DisposeCore()
            {
                parent.compositeDisposable.Remove(this);
            }
        }
    }
}
