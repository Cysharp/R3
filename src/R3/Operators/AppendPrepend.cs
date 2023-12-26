namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Append<T>(this Observable<T> source, T value)
    {
        return new AppendPrepend<T>(source, value, append: true);
    }

    public static Observable<T> Prepend<T>(this Observable<T> source, T value)
    {
        return new AppendPrepend<T>(source, value, append: false);
    }
}


internal sealed class AppendPrepend<T>(Observable<T> source, T value, bool append) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        if (!append) // prepend
        {
            observer.OnNext(value);
        }

        return source.Subscribe(new _Append(observer, value));
    }

    sealed class _Append(Observer<T> observer, T value) : Observer<T>
    {
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
            if (result.IsFailure)
            {
                observer.OnCompleted(result);
            }
            else
            {
                observer.OnNext(value);
                observer.OnCompleted();
            }
        }
    }
}
