namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> OfType<T, TResult>(this Observable<T> source)
    {
        return new OfType<T, TResult>(source);
    }
}

internal sealed class OfType<T, TResult>(Observable<T> source) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _OfType(observer));
    }

    sealed class _OfType(Observer<TResult> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            if (value is TResult v)
            {
                observer.OnNext(v);
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
