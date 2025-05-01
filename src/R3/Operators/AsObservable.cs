namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> AsObservable<T>(this Observable<T> source)
    {
        if (source is AsObservable<T>) // already hide
        {
            return source;
        }

        return new AsObservable<T>(source);
    }

    public static IObservable<T> AsSystemObservable<T>(this Observable<T> source)
    {
        return new AsSystemObservable<T>(source);
    }
}

internal sealed class AsObservable<T>(Observable<T> observable) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return observable.Subscribe(new AsObservableObserver(observer));
    }

    sealed class AsObservableObserver(Observer<T> observer) : Observer<T>
    {
        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            observer.Dispose();
        }
    }
}

internal sealed class AsSystemObservable<T>(Observable<T> source) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
    {
        return source.Subscribe(new ObserverToObserver(observer));
    }

    sealed class ObserverToObserver(IObserver<T> observer) : Observer<T>
    {
        protected override bool AutoDisposeOnCompleted => false;

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
