namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> TakeUntil<T, TOther>(this Observable<T> source, Observable<TOther> other)
    {
        return new TakeUntil<T, TOther>(source, other);
    }

    public static Observable<T> TakeUntil<T>(this Observable<T> source, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return source;
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return Observable.Empty<T>();
        }

        return new TakeUntilC<T>(source, cancellationToken);
    }

    public static Observable<T> TakeUntil<T>(this Observable<T> source, Task task)
    {
        return new TakeUntilT<T>(source, task);
    }

    public static Observable<T> TakeUntil<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait = true)
    {
        return new TakeUntilAsync<T>(source, asyncFunc, configureAwait);
    }
}

internal sealed class TakeUntil<T, TOther>(Observable<T> source, Observable<TOther> other) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var takeUntil = new _TakeUntil(observer);
        var stopperSubscription = other.Subscribe(takeUntil.stopper);
        try
        {
            return source.Subscribe(takeUntil); // subscription contains self and stopper.
        }
        catch
        {
            stopperSubscription.Dispose();
            throw;
        }
    }

    sealed class _TakeUntil : Observer<T>
    {
        readonly Observer<T> observer;
        internal readonly TakeUntilStopperObserver stopper;

        public _TakeUntil(Observer<T> observer)
        {
            this.observer = observer;
            this.stopper = new TakeUntilStopperObserver(this);
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
            stopper.Dispose();
        }
    }

    sealed class TakeUntilStopperObserver(_TakeUntil parent) : Observer<TOther>
    {
        protected override void OnNextCore(TOther value)
        {
            parent.OnCompleted(Result.Success);
            Dispose();
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

internal sealed class TakeUntilC<T>(Observable<T> source, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeUntil(observer, cancellationToken));
    }

    sealed class _TakeUntil : Observer<T>, IDisposable
    {
        static readonly Action<object?> cancellationCallback = CancellationCallback;

        readonly Observer<T> observer;
        CancellationTokenRegistration tokenRegistration;

        public _TakeUntil(Observer<T> observer, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.tokenRegistration = cancellationToken.Register(cancellationCallback, this);
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

        static void CancellationCallback(object? state)
        {
            var self = (_TakeUntil)state!;
            self.OnCompleted();
        }

        protected override void DisposeCore()
        {
            tokenRegistration.Dispose();
        }
    }
}

internal sealed class TakeUntilT<T>(Observable<T> source, Task task) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeUntil(observer, task));
    }

    sealed class _TakeUntil : Observer<T>, IDisposable
    {
        readonly Observer<T> observer;

        public _TakeUntil(Observer<T> observer, Task task)
        {
            this.observer = observer;
            TaskAwait(task);
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

        async void TaskAwait(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
                OnCompleted(Result.Success);
            }
            catch (Exception ex)
            {
                OnCompleted(Result.Failure(ex));
            }
        }
    }
}

internal sealed class TakeUntilAsync<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _TakeUntil(observer, asyncFunc, configureAwait));
    }

    sealed class _TakeUntil(Observer<T> observer, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait) : Observer<T>, IDisposable
    {
        readonly CancellationTokenSource cancellationTokenSource = new();
        int isTaskRunning;

        protected override void OnNextCore(T value)
        {
            var isFirstValue = (Interlocked.Exchange(ref isTaskRunning, 1) == 0);
            if (isFirstValue)
            {
                TaskStart(value);
            }

            observer.OnNext(value);
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

        async void TaskStart(T value)
        {

            try
            {
                await asyncFunc(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationTokenSource.Token)
                {
                    return;
                }

                // error is Stop
                observer.OnCompleted(Result.Failure(ex));
                return;
            }

            observer.OnCompleted();
        }
    }
}
