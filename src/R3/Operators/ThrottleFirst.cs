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

    public static Observable<T> ThrottleFirst<T, TSample>(this Observable<T> source, Observable<TSample> sampler)
    {
        return new ThrottleFirstObservableSampler<T, TSample>(source, sampler);
    }

    public static Observable<T> ThrottleFirst<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait = true)
    {
        return new ThrottleFirstAsyncSampler<T>(source, sampler, configureAwait);
    }
}

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
                    timer.InvokeOnce(timeSpan); // timer start before OnNext
                    observer.OnNext(value);     // call OnNext in lock
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

internal sealed class ThrottleFirstAsyncSampler<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleFirst(observer, sampler, configureAwait));
    }

    sealed class _ThrottleFirst(Observer<T> observer, Func<T, CancellationToken, ValueTask> sampler, bool configureAwait) : Observer<T>
    {
        readonly object gate = new object();
        readonly CancellationTokenSource cancellationTokenSource = new();
        bool closing;

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!closing)
                {
                    closing = true;
                    StartOpenGate(value);
                    observer.OnNext(value);
                }
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            cancellationTokenSource.Cancel(); // cancel executing async process first
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            cancellationTokenSource.Cancel();
        }

        async void StartOpenGate(T value)
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
                    closing = false;
                }
            }
        }
    }
}

internal sealed class ThrottleFirstObservableSampler<T, TSample>(Observable<T> source, Observable<TSample> sampler) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _ThrottleFirst(observer, sampler));
    }

    sealed class _ThrottleFirst : Observer<T>
    {
        readonly Observer<T> observer;
        readonly object gate = new object();
        readonly IDisposable samplerSubscription;
        bool closing;

        public _ThrottleFirst(Observer<T> observer, Observable<TSample> sampler)
        {
            this.observer = observer;
            var sampleObserver = new SamplerObserver(this);
            this.samplerSubscription = sampler.Subscribe(sampleObserver);
        }

        protected override void OnNextCore(T value)
        {
            lock (gate)
            {
                if (!closing)
                {
                    closing = true;
                    observer.OnNext(value);
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
            samplerSubscription.Dispose();
        }

        sealed class SamplerObserver(_ThrottleFirst parent) : Observer<TSample>
        {
            protected override void OnNextCore(TSample value)
            {
                lock (parent.gate)
                {
                    parent.closing = false; // open gate
                }
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
