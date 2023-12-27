namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<Unit> AsUnitObservable<T>(this Observable<T> source)
    {
        if (source is Observable<Unit> unit)
        {
            return unit;
        }

        return new AsUnitObservable<T>(source);
    }
}

internal sealed class AsUnitObservable<T>(Observable<T> source) : Observable<Unit>
{
    protected override IDisposable SubscribeCore(Observer<Unit> observer)
    {
        return source.Subscribe(new _AsUnitObservable(observer));
    }

    sealed class _AsUnitObservable(Observer<Unit> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(default);
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

