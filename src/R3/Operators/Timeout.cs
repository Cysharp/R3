namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Timeout<T>(this Observable<T> source, TimeSpan dueTime)
    {
        return new Timeout<T>(source, dueTime, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> Timeout<T>(this Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Timeout<T>(source, dueTime, timeProvider);
    }
}

internal sealed class Timeout<T>(Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Timeout(observer, dueTime.Normalize(), timeProvider));
    }

    sealed class _Timeout : Observer<T>
    {
        static readonly TimerCallback timerCallback = PublishTimeoutError;

        readonly Observer<T> observer;
        readonly TimeSpan timeSpan;
        readonly ITimer timer;
        readonly object gate = new object();
        int timerId;

        public _Timeout(Observer<T> observer, TimeSpan timeSpan, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timeSpan = timeSpan;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                Volatile.Write(ref timerId, unchecked(timerId + 1));
                observer.OnNext(value);
                timer.InvokeOnce(timeSpan); // restart timer
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

        protected override void DisposeCore()
        {
            timer.Dispose();
        }

        static void PublishTimeoutError(object? state)
        {
            var self = (_Timeout)state!;

            var timerId = Volatile.Read(ref self.timerId);
            lock (self.gate)
            {
                if (timerId != self.timerId) return;
                self.OnCompleted(new TimeoutException());
            }
        }
    }
}
