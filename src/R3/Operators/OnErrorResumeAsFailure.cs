namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> OnErrorResumeAsFailure<T>(this Observable<T> source)
    {
        return new OnErrorResumeAsFailure<T>(source);
    }
}

internal class OnErrorResumeAsFailure<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _OnErrorAsComplete(observer));
    }

    sealed class _OnErrorAsComplete(Observer<T> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnCompleted(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            observer.OnCompleted(complete);
        }
    }
}
