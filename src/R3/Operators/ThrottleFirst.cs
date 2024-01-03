namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> ThrottleFirst<T>(this Observable<T> source, TimeSpan timeSpan)
    {
        return new ThrottleFirst<T>(source, timeSpan, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> ThrottleFirst<T>(this Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
    {
        return new ThrottleFirst<T>(source, timeSpan, timeProvider);
    }
}

// ThrottleFirst
internal sealed class ThrottleFirst<T>(Observable<T> source, TimeSpan interval, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleFirst(observer, interval.Normalize(), timeProvider));
    }

    sealed class _ThrottleFirst : Observer<T>
    {
        static readonly TimerCallback timerCallback = RaiseOnNext;

        readonly Observer<T> observer;
        readonly ITimer timer;
        readonly object gate = new object();
        T? firstValue;
        bool hasValue;

        public _ThrottleFirst(Observer<T> observer, TimeSpan interval, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            this.timer.Change(interval, interval);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!hasValue)
                {
                    hasValue = true;
                    firstValue = value;
                }
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

        static void RaiseOnNext(object? state)
        {
            var self = (_ThrottleFirst)state!;
            lock (self.gate)
            {
                if (self.hasValue)
                {
                    self.observer.OnNext(self.firstValue!);
                    self.hasValue = false;
                    self.firstValue = default;
                }
            }
        }
    }
}
