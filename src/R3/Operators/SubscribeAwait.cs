using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, AwaitOperations awaitOperations = AwaitOperations.Queue, bool configureAwait = true)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), Stubs.HandleResult, awaitOperations, configureAwait);
    }

    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Result> onCompleted, AwaitOperations awaitOperations = AwaitOperations.Queue, bool configureAwait = true)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), onCompleted, awaitOperations, configureAwait);
    }

    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, AwaitOperations awaitOperations = AwaitOperations.Queue, bool configureAwait = true)
    {
        switch (awaitOperations)
        {
            case AwaitOperations.Queue:
                return source.Subscribe(new SubscribeAwaitQueue<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperations.Drop:
                return source.Subscribe(new SubscribeAwaitDrop<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperations.Parallel:
                return source.Subscribe(new SubscribeAwaitParallel<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            default:
                throw new ArgumentException();
        }
    }
}

internal sealed class SubscribeAwaitQueue<T> : Observer<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;
    readonly bool configureAwait; // continueOnCapturedContext

    readonly CancellationTokenSource cancellationTokenSource;
    readonly Channel<T> channel;

    public SubscribeAwaitQueue(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
        this.configureAwait = configureAwait;

        this.cancellationTokenSource = new CancellationTokenSource();
        this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

        RunQueueWorker(); // start reader loop
    }

    protected override void OnNextCore(T value)
    {
        channel.Writer.TryWrite(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        onCompleted(result);
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

                        await onNextAsync(item, token).ConfigureAwait(configureAwait);
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                        {
                            return;
                        }
                        OnErrorResume(ex);
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

internal sealed class SubscribeAwaitDrop<T> : Observer<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;
    readonly bool configureAwait; // continueOnCapturedContext

    readonly CancellationTokenSource cancellationTokenSource;
    int runningState; // 0 = stopped, 1 = running

    public SubscribeAwaitDrop(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
        this.configureAwait = configureAwait;

        this.cancellationTokenSource = new CancellationTokenSource();
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
        onErrorResume(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        onCompleted(result);
    }

    protected override void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    async void StartAsync(T value)
    {
        try
        {
            await onNextAsync(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return;
            }
            OnErrorResume(ex);
        }
        finally
        {
            Interlocked.Exchange(ref runningState, 0);
        }
    }
}

sealed class SubscribeAwaitParallel<T> : Observer<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;
    readonly bool configureAwait; // continueOnCapturedContext

    readonly CancellationTokenSource cancellationTokenSource;
    readonly object gate = new object();

    public SubscribeAwaitParallel(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
        this.configureAwait = configureAwait;

        this.cancellationTokenSource = new CancellationTokenSource();
    }

    protected override void OnNextCore(T value)
    {
        StartAsync(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        lock (gate)
        {
            onErrorResume(error);
        }
    }

    protected override void OnCompletedCore(Result result)
    {
        onCompleted(result);
    }

    protected override void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    async void StartAsync(T value)
    {
        try
        {
            await onNextAsync(value, cancellationTokenSource.Token).ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return;
            }
            OnErrorResume(ex);
        }
    }
}
