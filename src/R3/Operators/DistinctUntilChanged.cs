namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> DistinctUntilChanged<T>(this Observable<T> source)
    {
        return DistinctUntilChanged(source, EqualityComparer<T>.Default);
    }

    public static Observable<T> DistinctUntilChanged<T>(this Observable<T> source, IEqualityComparer<T> comparer)
    {
        return new DistinctUntilChanged<T>(source, comparer);
    }

    public static Observable<T> DistinctUntilChangedBy<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector)
    {
        return DistinctUntilChangedBy(source, keySelector, EqualityComparer<TKey>.Default);
    }

    public static Observable<T> DistinctUntilChangedBy<T, TKey>(this Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        return new DistinctUntilChangedBy<T, TKey>(source, keySelector, comparer);
    }
}

internal sealed class DistinctUntilChanged<T>(Observable<T> source, IEqualityComparer<T> comparer) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _DistinctUntilChanged(observer, comparer));
    }

    sealed class _DistinctUntilChanged : Observer<T>
    {
        readonly Observer<T> observer;
        readonly IEqualityComparer<T> comparer;
        T? lastValue;
        bool hasValue;

        public _DistinctUntilChanged(Observer<T> observer, IEqualityComparer<T> comparer)
        {
            this.observer = observer;
            this.comparer = comparer;
        }

        protected override void OnNextCore(T value)
        {
            if (!hasValue || !comparer.Equals(lastValue!, value))
            {
                hasValue = true;
                lastValue = value;
                observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}

internal sealed class DistinctUntilChangedBy<T, TKey>(Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _DistinctUntilChangedBy(observer, keySelector, comparer));
    }

    sealed class _DistinctUntilChangedBy : Observer<T>
    {
        readonly Observer<T> observer;
        readonly Func<T, TKey> keySelector;
        readonly IEqualityComparer<TKey> comparer;
        TKey? lastKey;
        bool hasValue;

        public _DistinctUntilChangedBy(Observer<T> observer, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            this.observer = observer;
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        protected override void OnNextCore(T value)
        {
            var key = keySelector(value);
            if (!hasValue || !comparer.Equals(lastKey!, key))
            {
                hasValue = true;
                lastKey = key;
                observer.OnNext(value);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}
