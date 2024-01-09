namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Distinct<T>(this Observable<T> source)
    {
        return Distinct(source, EqualityComparer<T>.Default);
    }

    public static Observable<T> Distinct<T>(this Observable<T> source, IEqualityComparer<T> comparer)
    {
        return new Distinct<T>(source, comparer);
    }

    public static Observable<TSource> DistinctBy<TSource, TKey>(this Observable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return DistinctBy(source, keySelector, EqualityComparer<TKey>.Default);
    }

    public static Observable<TSource> DistinctBy<TSource, TKey>(this Observable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        return new DistinctBy<TSource, TKey>(source, keySelector, comparer);
    }
}

internal sealed class Distinct<T>(Observable<T> source, IEqualityComparer<T> comparer) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Distinct(observer, comparer));
    }

    sealed class _Distinct : Observer<T>
    {
        readonly Observer<T> observer;
        readonly HashSet<T> set;

        public _Distinct(Observer<T> observer, IEqualityComparer<T> comparer)
        {
            this.observer = observer;
            this.set = new HashSet<T>(comparer);
        }

        protected override void OnNextCore(T value)
        {
            if (set.Add(value))
            {
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

internal sealed class DistinctBy<T, TKey>(Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _DistinctBy(observer, keySelector, comparer));
    }

    sealed class _DistinctBy : Observer<T>
    {
        readonly Observer<T> observer;
        readonly Func<T, TKey> keySelector;
        readonly HashSet<TKey> set;

        public _DistinctBy(Observer<T> observer, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            this.observer = observer;
            this.keySelector = keySelector;
            this.set = new HashSet<TKey>(comparer);
        }

        protected override void OnNextCore(T value)
        {
            var key = keySelector(value);
            if (set.Add(key))
            {
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
