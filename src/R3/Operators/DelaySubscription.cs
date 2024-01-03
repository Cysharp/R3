namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> DelaySubscription<T>(this Observable<T> source, TimeSpan dueTime)
    {
        return new DelaySubscription<T>(source, dueTime, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> DelaySubscription<T>(this Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new DelaySubscription<T>(source, dueTime, timeProvider);
    }
}

internal sealed class DelaySubscription<T>(Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _DelaySubscription(observer, source, dueTime.Normalize(), timeProvider).Run();
    }

    sealed class _DelaySubscription : Observer<T>
    {
        static readonly TimerCallback timerCallback = Subscribe;

        readonly Observer<T> observer;
        readonly Observable<T> source;
        readonly TimeSpan dueTime;
        readonly ITimer timer;

        public _DelaySubscription(Observer<T> observer, Observable<T> source, TimeSpan dueTime, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.source = source;
            this.dueTime = dueTime;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        public IDisposable Run()
        {
            timer.InvokeOnce(dueTime);
            return this;
        }

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
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            timer.Dispose();
        }

        static void Subscribe(object? state)
        {
            var self = (_DelaySubscription)state!;
            try
            {
                self.source.Subscribe(self); // subscribe self.
            }
            catch (Exception ex)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                self.Dispose();
            }
        }
    }
}
