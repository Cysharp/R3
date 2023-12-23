namespace R3;

public static partial class ObservableExtensions
{
    // TODO: test

    public static Observable<T> AsObservable<T>(this Observable<T> source)
    {
        if (source is AsObservable<T>) // already hide
        {
            return source;
        }

        return new AsObservable<T>(source);
    }

    public static IObservable<T> AsIObservable<T>(this Observable<T> source)
    {
        return new AsIObservable<T>(source);
    }
}

internal sealed class AsObservable<T>(Observable<T> observable) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return observable.Subscribe(observer.Wrap());
    }
}

internal sealed class AsIObservable<T>(Observable<T> source) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
    {
        return source.Subscribe(new ObserverToObserver(observer));
    }

    sealed class ObserverToObserver(IObserver<T> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnError(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                observer.OnError(result.Exception);
            }
            else
            {
                observer.OnCompleted();
            }
        }
    }
}
