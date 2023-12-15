namespace R3;

public static partial class ObservableExtensions
{
    // TODO: more overload?
    public static IObservable<T> ToIObservable<T>(this Observable<T> source)
    {
        return new ToIObservable<T>(source);
    }
}

internal sealed class ToIObservable<T>(Observable<T> source) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
    {
        return source.Subscribe(new ObserverToobserver(observer));
    }

    sealed class ObserverToobserver(IObserver<T> observer) : Observer<T>
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
