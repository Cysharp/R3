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
internal sealed class ThrottleFirst<T>(Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleFirst(observer, timeSpan.Normalize(), timeProvider));
    }

    sealed class _ThrottleFirst : Observer<T>
    {
        static readonly TimerCallback timerCallback = OpenGate;

        readonly Observer<T> observer;
        readonly ITimer timer;
        readonly TimeSpan timeSpan;
        readonly object gate = new object();
        bool closing;

        public _ThrottleFirst(Observer<T> observer, TimeSpan timeSpan, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
            this.timeSpan = timeSpan;
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!closing)
                {
                    closing = true;
                    observer.OnNext(value);
                    timer.InvokeOnce(timeSpan);
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

        static void OpenGate(object? state)
        {
            var self = (_ThrottleFirst)state!;
            lock (self.gate)
            {
                self.closing = false;
            }
        }
    }
}
