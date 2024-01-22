using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> WhereAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperations awaitOperations = AwaitOperations.Queue, bool configureAwait = true)
    {
        return new WhereAwait<T>(source, predicate, awaitOperations, configureAwait);
    }
}

internal sealed class WhereAwait<T>(Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperations awaitOperations, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        switch (awaitOperations)
        {
            case AwaitOperations.Queue:
                return source.Subscribe(new WhereAwaitQueue(observer, predicate, configureAwait));
            case AwaitOperations.Drop:
                return source.Subscribe(new WhereAwaitDrop(observer, predicate, configureAwait));
            case AwaitOperations.Parallel:
                return source.Subscribe(new WhereAwaitParallel(observer, predicate, configureAwait));
            default:
                throw new ArgumentException();
        }
    }

    sealed class WhereAwaitQueue : Observer<T>
    {
        readonly Observer<T> observer;
        readonly Func<T, CancellationToken, ValueTask<bool>> predicate;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext
        readonly Channel<T> channel;

        public WhereAwaitQueue(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait)
        {
            this.observer = observer;
            this.predicate = predicate;
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

                            var result = await predicate(item, token).ConfigureAwait(configureAwait);
                            if (result)
                            {
                                observer.OnNext(item);
                            }
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

    sealed class WhereAwaitDrop : Observer<T>
    {
        readonly Observer<T> observer;
        readonly Func<T, CancellationToken, ValueTask<bool>> predicate;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext

        int runningState; // 0 = stopped, 1 = running

        public WhereAwaitDrop(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait)
        {
            this.observer = observer;
            this.predicate = predicate;
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
                var result = await predicate(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
                if (result)
                {
                    observer.OnNext(value);
                }
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

    sealed class WhereAwaitParallel : Observer<T>
    {
        readonly Observer<T> observer;
        readonly Func<T, CancellationToken, ValueTask<bool>> predicate;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly bool configureAwait; // continueOnCapturedContext
        readonly object gate = new object();

        public WhereAwaitParallel(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait)
        {
            this.observer = observer;
            this.predicate = predicate;
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
                var result = await predicate(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
                if (result)
                {
                    lock (gate)
                    {
                        observer.OnNext(value);
                    }
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
