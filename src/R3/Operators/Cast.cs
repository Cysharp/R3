namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> Cast<T, TResult>(this Observable<T> source)
    {
        return new Cast<T, TResult>(source);
    }
}

internal sealed class Cast<T, TResult>(Observable<T> source) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _Cast(observer));
    }

    sealed class _Cast(Observer<TResult> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            var v = (TResult?)(object?)value;
            observer.OnNext(v!);
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

