using System.Threading.Channels;

namespace R3;

public enum AwaitOperation
{
    /// <summary>All values are queued, and the next value waits for the completion of the asynchronous method.</summary>
    Sequential,
    /// <summary>Drop new value when async operation is running.</summary>
    Drop,
    /// <summary>If the previous asynchronous method is running, it is cancelled and the next asynchronous method is executed.</summary>
    Switch,
    /// <summary>All values are sent immediately to the asynchronous method.</summary>
    Parallel,
    /// <summary>All values are sent immediately to the asynchronous method, but the results are queued and passed to the next operator in order.</summary>
    SequentialParallel
}

internal abstract class AwaitOperationSequentialObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    readonly Channel<T> channel;
    bool completed;

    protected override bool AutoDisposeOnCompleted => false; // disable auto-dispose

    public AwaitOperationSequentialObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
        this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

        RunQueueWorker();
    }

    protected override sealed void OnNextCore(T value)
    {
        channel.Writer.TryWrite(value);
    }

    protected override sealed void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }

        Volatile.Write(ref completed, true);
        channel.Writer.Complete(); // exit wait read loop
    }

    protected override sealed void DisposeCore()
    {
        channel.Writer.Complete(); // complete writing
        cancellationTokenSource.Cancel(); // stop selector await.
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);
    protected abstract void PublishOnCompleted(Result result);

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

                        await OnNextAsync(item, token, configureAwait).ConfigureAwait(configureAwait);
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

            if (Volatile.Read(ref completed))
            {
                PublishOnCompleted(Result.Success);
                Dispose();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
    }
}

internal abstract class AwaitOperationDropObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    int runningState; // 0 = stopped, 1 = running, 2 = complete

    protected sealed override bool AutoDisposeOnCompleted => false; // disable auto-dispose

    public AwaitOperationDropObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
    }

    protected override sealed void OnNextCore(T value)
    {
        if (Interlocked.CompareExchange(ref runningState, 1, 0) == 0)
        {
            StartAsync(value);
        }
    }

    protected override sealed void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }

        if (Interlocked.Exchange(ref runningState, 2) == 0)
        {
            PublishOnCompleted(result);
        }
    }

    protected override sealed void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);
    protected abstract void PublishOnCompleted(Result result);

    async void StartAsync(T value)
    {
        try
        {
            await OnNextAsync(value, cancellationTokenSource.Token, configureAwait).ConfigureAwait(configureAwait);
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
            if (Interlocked.CompareExchange(ref runningState, 0, 1) == 2)
            {
                PublishOnCompleted(Result.Success);
            }
        }
    }
}

internal abstract class AwaitOperationSwitchObserver<T> : Observer<T>
{
    CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    protected readonly object gate = new object();
    bool running;
    bool completed;

    protected sealed override bool AutoDisposeOnCompleted => false; // disable auto-dispose

    public AwaitOperationSwitchObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
    }

    protected override sealed void OnNextCore(T value)
    {
        CancellationToken token = cancellationTokenSource.Token;
        lock (gate)
        {
            if (running)
            {
                if (IsDisposed) return;
                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                token = cancellationTokenSource.Token;
            }
            running = true;
        }

        StartAsync(value, token);
    }

    protected override sealed void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }

        lock (gate)
        {
            if (running)
            {
                completed = true;
            }
            else
            {
                PublishOnCompleted(result);
                Dispose();
                return;
            }
        }
    }

    protected override void DisposeCore()
    {
        lock (gate)
        {
            cancellationTokenSource.Cancel();
        }
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);
    protected abstract void PublishOnCompleted(Result result);

    async void StartAsync(T value, CancellationToken token)
    {
        try
        {
            await OnNextAsync(value, token, configureAwait).ConfigureAwait(configureAwait);
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
            lock (gate)
            {
                if (!token.IsCancellationRequested)
                {
                    running = false;
                    if (completed)
                    {
                        PublishOnCompleted(Result.Success);
                        Dispose();
                    }
                }
            }
        }
    }
}

internal abstract class AwaitOperationParallelObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    protected readonly object gate = new object(); // need to use gate.

    protected sealed override bool AutoDisposeOnCompleted => false; // disable auto-dispose
    int runningCount = 0;
    bool completed;

    public AwaitOperationParallelObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
    }

    protected override sealed void OnNextCore(T value)
    {
        Interlocked.Increment(ref runningCount);
        StartAsync(value);
    }

    protected override sealed void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }

        Volatile.Write(ref completed, true);
        if (Volatile.Read(ref runningCount) == 0)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }
    }

    protected override void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);
    protected abstract void PublishOnCompleted(Result result);

    async void StartAsync(T value)
    {
        try
        {
            await OnNextAsync(value, cancellationTokenSource.Token, configureAwait).ConfigureAwait(configureAwait);
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
            if (Interlocked.Decrement(ref runningCount) == 0 && Volatile.Read(ref completed))
            {
                PublishOnCompleted(Result.Success);
                Dispose();
            }
        }
    }
}

internal abstract class AwaitOperationSequentialParallelObserver<T, TTaskValue> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    readonly Channel<(T, ValueTask<TTaskValue>)> channel;
    bool completed;

    protected sealed override bool AutoDisposeOnCompleted => false; // disable auto-dispose

    public AwaitOperationSequentialParallelObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
        this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<(T, ValueTask<TTaskValue>)>();

        RunQueueWorker();
    }

    protected override sealed void OnNextCore(T value)
    {
        var task = OnNextTaskAsync(value, cancellationTokenSource.Token, configureAwait);
        channel.Writer.TryWrite((value, task));
    }

    protected override sealed void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            PublishOnCompleted(result);
            Dispose();
            return;
        }

        Volatile.Write(ref completed, true);
        channel.Writer.Complete(); // exit wait read loop
    }

    protected override sealed void DisposeCore()
    {
        channel.Writer.Complete(); // complete writing
        cancellationTokenSource.Cancel(); // stop selector await.
    }

    protected abstract ValueTask<TTaskValue> OnNextTaskAsync(T value, CancellationToken cancellationToken, bool configureAwait);
    protected abstract void PublishOnNext(T value, TTaskValue result);
    protected abstract void PublishOnCompleted(Result result);

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

                        var result = await item.Item2.ConfigureAwait(configureAwait);
                        PublishOnNext(item.Item1, result);
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

            if (Volatile.Read(ref completed))
            {
                PublishOnCompleted(Result.Success);
                Dispose();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
        }
    }
}
