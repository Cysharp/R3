namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<int> Index(this Observable<Unit> source)
    {
        return new IndexObservable(source);
    }

    public static Observable<(int Index, T Item)> Index<T>(this Observable<T> source)
    {
        return new IndexObservable<T>(source);
    }
}

internal sealed class IndexObservable(Observable<Unit> source) : Observable<int>
{
    protected override IDisposable SubscribeCore(Observer<int> observer)
    {
        return source.Subscribe(new _Index(observer));
    }

    sealed class _Index(Observer<int> observer) : Observer<Unit>
    {
        int index = -1;

        protected override void OnNextCore(Unit value)
        {
            checked
            {
                index++;
            }
            observer.OnNext(index);
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

internal sealed class IndexObservable<T>(Observable<T> source) : Observable<(int Index, T Item)>
{
    protected override IDisposable SubscribeCore(Observer<(int Index, T Item)> observer)
    {
        return source.Subscribe(new _Index(observer));
    }

    sealed class _Index(Observer<(int Index, T Item)> observer) : Observer<T>
    {
        int index = -1;

        protected override void OnNextCore(T value)
        {
            checked
            {
                index++;
            }
            observer.OnNext((index, value));
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
