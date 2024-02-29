namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> ThrottleLast<T>(this Observable<T> source, TimeSpan timeSpan)
    {
        return new ThrottleLast<T>(source, timeSpan, ObservableSystem.DefaultTimeProvider);
    }

    public static Observable<T> ThrottleLast<T>(this Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
    {
        return new ThrottleLast<T>(source, timeSpan, timeProvider);
    }

    public static Observable<T> ThrottleLast<T, TSample>(this Observable<T> source, Observable<TSample> sampler)
    {
        return new ThrottleLastObservableSampler<T, TSample>(source, sampler);
    }

    public static Observable<T> ThrottleLast<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait = true)
    {
        return new ThrottleLastAsyncSampler<T>(source, sampler, configureAwait);
    }
}

internal sealed class ThrottleLast<T>(Observable<T> source, TimeSpan interval, TimeProvider timeProvider) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleLast(observer, interval.Normalize(), timeProvider));
    }

    sealed class _ThrottleLast : Observer<T>
    {
        static readonly TimerCallback timerCallback = RaiseOnNext;

        readonly Observer<T> observer;
        readonly TimeSpan interval;
        readonly ITimer timer;
        readonly object gate = new object();
        T? lastValue;
        bool hasValue;

        public _ThrottleLast(Observer<T> observer, TimeSpan interval, TimeProvider timeProvider)
        {
            this.observer = observer;
            this.interval = interval;
            this.timer = timeProvider.CreateStoppedTimer(timerCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!hasValue) // timer is stopping
                {
                    timer.InvokeOnce(interval);
                }

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
            var self = (_ThrottleLast)state!;
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

internal sealed class ThrottleLastAsyncSampler<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleLast(observer, sampler, configureAwait));
    }

    sealed class _ThrottleLast(Observer<T> observer, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait) : Observer<T>
    {
        readonly object gate = new object();
        readonly CancellationTokenSource cancellationTokenSource = new();
        T? lastValue;
        bool isRunning;

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                lastValue = value;
                if (!isRunning)
                {
                    isRunning = true;
                    RaiseOnNextAsync(value);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            cancellationTokenSource.Cancel();
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            cancellationTokenSource.Cancel();
        }

        async void RaiseOnNextAsync(T value)
        {
            try
            {
                await sampler(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationTokenSource.Token)
                {
                    return;
                }
                OnErrorResume(ex);
            }
            finally
            {
                lock (gate)
                {
                    observer.OnNext(lastValue!);
                    lastValue = default;
                    isRunning = false;
                }
            }
        }
    }
}

internal sealed class ThrottleLastObservableSampler<T, TSample>(Observable<T> source, Observable<TSample> sampler) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleLast(observer, sampler));
    }

    sealed class _ThrottleLast : Observer<T>
    {
        readonly Observer<T> observer;
        readonly object gate = new object();
        readonly IDisposable samplerSubscription;
        T? lastValue;
        bool hasValue;

        public _ThrottleLast(Observer<T> observer, Observable<TSample> sampler)
        {
            this.observer = observer;
            var sampleObserver = new SamplerObserver(this);
            this.samplerSubscription = sampler.Subscribe(sampleObserver);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                lastValue = value;
                hasValue = true;
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
            samplerSubscription.Dispose();
        }

        void PublishOnNext()
        {
            lock (gate)
            {
                if (hasValue)
                {
                    observer.OnNext(lastValue!);
                    hasValue = false;
                    lastValue = default;
                }
            }
        }

        sealed class SamplerObserver(_ThrottleLast parent) : Observer<TSample>
        {
            protected override void OnNextCore(TSample value)
            {
                parent.PublishOnNext();
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                parent.OnCompleted(result);
            }
        }
    }
}
