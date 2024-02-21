namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<(long Timestamp, T Value)> Timestamp<T>(this Observable<T> source)
    {
        return new Timestamp<T>(source, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<(long Timestamp, T Value)> Timestamp<T>(this Observable<T> source, TimeProvider timeProvider)
    {
        return new Timestamp<T>(source, timeProvider);
    }
}

internal sealed class Timestamp<T>(Observable<T> source, TimeProvider timeProvider) : Observable<(long Timestamp, T Value)>
{
    protected override IDisposable SubscribeCore(Observer<(long Timestamp, T Value)> observer)
    {
        return source.Subscribe(new _Timestamp(observer, timeProvider));
    }

    sealed class _Timestamp(Observer<(long Timestamp, T Value)> observer, TimeProvider timeProvider) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext((timeProvider.GetTimestamp(), value));
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
