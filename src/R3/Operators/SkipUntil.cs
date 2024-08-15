namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> SkipUntil<T, TOther>(this Observable<T> source, Observable<TOther> other)
    {
        return new SkipUntil<T, TOther>(source, other);
    }

    public static Observable<T> SkipUntil<T>(this Observable<T> source, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled) throw new ArgumentException("cancellationToken must be cancellable", nameof(cancellationToken));
        return new SkipUntilC<T>(source, cancellationToken);
    }

    public static Observable<T> SkipUntil<T>(this Observable<T> source, Task task, bool configureAwait = true)
    {
        return new SkipUntilT<T>(source, task, configureAwait);
    }

    public static Observable<T> SkipUntil<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait = true)
    {
        return new SkipUntilAsync<T>(source, asyncFunc, configureAwait);
    }
}

internal sealed class SkipUntil<T, TOther>(Observable<T> source, Observable<TOther> other) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        var skipUntil = new _SkipUntil(observer);
        var otherSubscription = other.Subscribe(skipUntil.otherObserver);
        try
        {
            return source.Subscribe(skipUntil); // subscription contains self and other.
        }
        catch
        {
            otherSubscription.Dispose();
            throw;
        }
    }

    sealed class _SkipUntil : Observer<T>
    {
        readonly Observer<T> observer;
        internal readonly SkipUntilOtherObserver otherObserver;

        internal bool open;

        public _SkipUntil(Observer<T> observer)
        {
            this.observer = observer;
            this.otherObserver = new SkipUntilOtherObserver(this);
        }

        protected override void OnNextCore(T value)
        {
            if (Volatile.Read(ref open))
            {
                observer.OnNext(value);
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
            otherObserver.Dispose();
        }
    }

    sealed class SkipUntilOtherObserver(_SkipUntil parent) : Observer<TOther>
    {
        protected override void OnNextCore(TOther value)
        {
            Volatile.Write(ref parent.open, true);
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

internal sealed class SkipUntilC<T>(Observable<T> source, CancellationToken cancellationToken) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipUntil(observer, cancellationToken));
    }

    sealed class _SkipUntil : Observer<T>, IDisposable
    {
        static readonly Action<object?> cancellationCallback = CancellationCallback;

        readonly Observer<T> observer;
        CancellationTokenRegistration tokenRegistration;
        bool open;

        public _SkipUntil(Observer<T> observer, CancellationToken cancellationToken)
        {
            this.observer = observer;
            this.tokenRegistration = cancellationToken.Register(cancellationCallback, this);
        }

        protected override void OnNextCore(T value)
        {
            if (Volatile.Read(ref open))
            {
                observer.OnNext(value);
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

        static void CancellationCallback(object? state)
        {
            var self = (_SkipUntil)state!;
            Volatile.Write(ref self.open, true);
        }

        protected override void DisposeCore()
        {
            tokenRegistration.Dispose();
        }
    }
}

internal sealed class SkipUntilT<T>(Observable<T> source, Task task, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipUntil(observer, task, configureAwait));
    }

    sealed class _SkipUntil : Observer<T>, IDisposable
    {
        readonly bool configureAwait;
        readonly Observer<T> observer;
        bool open;

        public _SkipUntil(Observer<T> observer, Task task, bool configureAwait)
        {
            this.configureAwait = configureAwait;
            this.observer = observer;
            TaskAwait(task);
        }

        protected override void OnNextCore(T value)
        {
            if (Volatile.Read(ref open))
            {
                observer.OnNext(value);
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

        async void TaskAwait(Task task)
        {
            try
            {
                await task.ConfigureAwait(configureAwait);
                Volatile.Write(ref open, true);
            }
            catch (Exception ex)
            {
                OnCompleted(Result.Failure(ex));
            }
        }
    }
}

internal sealed class SkipUntilAsync<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _SkipUntil(observer, asyncFunc, configureAwait));
    }

    sealed class _SkipUntil(Observer<T> observer, Func<T, CancellationToken, ValueTask> asyncFunc, bool configureAwait) : Observer<T>, IDisposable
    {
        readonly CancellationTokenSource cancellationTokenSource = new();
        int isTaskRunning;
        bool open;

        protected override void OnNextCore(T value)
        {
            var isFirstValue = (Interlocked.Exchange(ref isTaskRunning, 1) == 0);
            if (isFirstValue)
            {
                TaskStart(value);
            }

            if (Volatile.Read(ref open))
            {
                observer.OnNext(value);
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

            Volatile.Write(ref open, true);
        }
    }
}

