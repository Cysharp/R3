using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> SelectAwait<T, TResult>(this Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperations awaitOperations = AwaitOperations.Queue, bool configureAwait = true)
    {
        return new SelectAwait<T, TResult>(source, selector, awaitOperations, configureAwait);
    }
}

internal sealed class SelectAwait<T, TResult>(Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperations awaitOperations, bool configureAwait) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        switch (awaitOperations)
        {
            case AwaitOperations.Queue:
                return source.Subscribe(new SelectAwaitQueue(observer, selector, configureAwait));
            case AwaitOperations.Drop:
                return source.Subscribe(new SelectAwaitDrop(observer, selector, configureAwait));
            case AwaitOperations.Parallel:
                return source.Subscribe(new SelectAwaitParallel(observer, selector, configureAwait));
            default:
                throw new ArgumentException();
        }
    }

    sealed class SelectAwaitQueue : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext
        readonly Channel<T> channel;

        public SelectAwaitQueue(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.configureAwait = configureAwait;
            this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

            RunQueueWorker(); // start reader loop
        }

        protected override void OnNextCore(T value)
        {
            channel.Writer.TryWrite(value);
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
            channel.Writer.Complete(); // complete writing
            cancellationTokenSource.Cancel(); // stop selector await.
        }

        async void RunQueueWorker() // don't(can't) wait so use async void
        {
            var reader = channel.Reader;
            var token = cancellationTokenSource.Token;

            try
            {
                while (await reader.WaitToReadAsync(/* don't pass CancellationToken, uses WriterComplete */).ConfigureAwait(configureAwait))
                {
                    while (reader.TryRead(out var item))
                    {
                        try
                        {
                            if (token.IsCancellationRequested) return;

                            var value = await selector(item, token).ConfigureAwait(configureAwait);
                            observer.OnNext(value);
                        }
                        catch (Exception ex)
                        {
                            if (ex is OperationCanceledException)
                            {
                                return;
                            }
                            observer.OnErrorResume(ex);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
            }
        }
    }

    sealed class SelectAwaitDrop : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext

        int runningState; // 0 = stopped, 1 = running

        public SelectAwaitDrop(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.configureAwait = configureAwait;
        }

        protected override void OnNextCore(T value)
        {
            if (Interlocked.CompareExchange(ref runningState, 1, 0) == 0)
            {
                StartAsync(value);
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
            cancellationTokenSource.Cancel();
        }

        async void StartAsync(T value)
        {
            try
            {
                var v = await selector(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
                observer.OnNext(v);
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    return;
                }
                observer.OnErrorResume(ex);
            }
            finally
            {
                Interlocked.Exchange(ref runningState, 0);
            }
        }
    }

    sealed class SelectAwaitParallel : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext
        readonly object gate = new object();

        public SelectAwaitParallel(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
            this.cancellationTokenSource = new CancellationTokenSource();
            this.configureAwait = configureAwait;
        }

        protected override void OnNextCore(T value)
        {
            StartAsync(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            cancellationTokenSource.Cancel();
        }

        async void StartAsync(T value)
        {
            try
            {
                var v = await selector(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
                lock (gate)
                {
                    observer.OnNext(v);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    return;
                }
                lock (gate)
                {
                    observer.OnErrorResume(ex);
                }
            }
        }
    }
}
