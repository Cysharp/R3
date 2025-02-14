namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T[]> ChunkUntil<T>(this Observable<T> source, Func<T, bool> predicate)
    {
        return new ChunkUntil<T>(source, predicate);
    }

    public static Observable<T[]> ChunkUntil<T>(this Observable<T> source, Func<T, int, bool> predicate)
    {
        return new ChunkUntilI<T>(source, predicate);
    }
}

internal sealed class ChunkUntil<T>(Observable<T> source, Func<T, bool> predicate) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _ChunkUntil(observer, predicate));
    }

    sealed class _ChunkUntil(Observer<T[]> observer, Func<T, bool> predicate) : Observer<T>, IDisposable
    {
        readonly List<T> list = new List<T>();

        protected override void OnNextCore(T value)
        {
            list.Add(value);
            if (predicate(value))
            {
                var array = list.ToArray();
                list.Clear();
                observer.OnNext(array);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (list.Count > 0)
            {
                observer.OnNext(list.ToArray());
            }

            observer.OnCompleted(result);
        }
    }
}

internal sealed class ChunkUntilI<T>(Observable<T> source, Func<T, int, bool> predicate) : Observable<T[]>
{
    protected override IDisposable SubscribeCore(Observer<T[]> observer)
    {
        return source.Subscribe(new _ChunkUntil(observer, predicate));
    }

    sealed class _ChunkUntil(Observer<T[]> observer, Func<T, int, bool> predicate) : Observer<T>, IDisposable
    {
        int count;
        readonly List<T> list = new List<T>();

        protected override void OnNextCore(T value)
        {
            list.Add(value);
            if (predicate(value, count++))
            {
                var array = list.ToArray();
                list.Clear();
                observer.OnNext(array);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (list.Count > 0)
            {
                observer.OnNext(list.ToArray());
            }

            observer.OnCompleted(result);
        }
    }
}
