namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Sample<T>(this Observable<T> source, TimeSpan timeSpan)
    {
        return new Sample<T>(source, timeSpan, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> Sample<T>(this Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
    {
        return new Sample<T>(source, timeSpan, timeProvider);
    }
}

// Sample(ThrottleLast)
internal sealed class Sample<T>(Observable<T> source, TimeSpan interval, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Sample(observer, interval.Normalize(), timeProvider));
    }

    sealed class _Sample : Observer<T>
    {
        static readonly TimerCallback timerCallback = RaiseOnNext;

        readonly Observer<T> observer;
        readonly ITimer timer;
        readonly object gate = new object();
        T? lastValue;
        bool hasValue;

        public _Sample(Observer<T> observer, TimeSpan interval, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            this.timer.Change(interval, interval);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                hasValue = true;
                lastValue = value;
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
            var self = (_Sample)state!;
            lock (self.gate)
            {
                if (self.hasValue)
                {
                    self.observer.OnNext(self.lastValue!);
                    self.hasValue = false;
                    self.lastValue = default;
                }
            }
        }
    }
}
