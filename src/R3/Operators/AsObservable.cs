namespace R3;

public static partial class ObservableExtensions
{
    // TODO: more overload?
    public static IObservable<T> AsObservable<T>(this Observable<T> source)
    {
        return new AsObservable<T>(source);
    }
}

internal sealed class AsObservable<T>(Observable<T> source) : IObservable<T>
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
