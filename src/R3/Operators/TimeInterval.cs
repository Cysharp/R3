namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<(TimeSpan Interval, T Value)> TimeInterval<T>(this Observable<T> source)
    {
        return new TimeInterval<T>(source, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<(TimeSpan Interval, T Value)> TimeInterval<T>(this Observable<T> source, TimeProvider timeProvider)
    {
        return new TimeInterval<T>(source, timeProvider);
    }
}

internal sealed class TimeInterval<T>(Observable<T> source, TimeProvider timeProvider) : Observable<(TimeSpan Interval, T Value)>
{
    protected override IDisposable SubscribeCore(Observer<(TimeSpan Interval, T Value)> observer)
    {
        return source.Subscribe(new _TimeInterval(observer, timeProvider));
    }

    sealed class _TimeInterval(Observer<(TimeSpan Interval, T Value)> observer, TimeProvider timeProvider) : Observer<T>
    {
        long previousTimestamp = timeProvider.GetTimestamp();

        protected override void OnNextCore(T value)
        {
            var currentTimestamp = timeProvider.GetTimestamp();
            var elapsed = timeProvider.GetElapsedTime(previousTimestamp, currentTimestamp);
            this.previousTimestamp = currentTimestamp;

            observer.OnNext((elapsed, value));
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
