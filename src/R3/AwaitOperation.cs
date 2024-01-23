using System;
using System.Threading.Channels;

namespace R3;

public enum AwaitOperation
{
    Sequential, // TODO: -> Sequential
    Drop,
    // TODO: Switch
    Parallel
    // TODO: SequentialParallel
}

internal abstract class AwaitOperationSequentialObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    readonly Channel<T> channel;

    public AwaitOperationSequentialObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
        this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

        RunQueueWorker();
    }

    protected override void OnNextCore(T value)
    {
        channel.Writer.TryWrite(value);
    }

    protected override void DisposeCore()
    {
        channel.Writer.Complete(); // complete writing
        cancellationTokenSource.Cancel(); // stop selector await.
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);

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

                        await OnNextAsync(item, token, configureAwait).ConfigureAwait(false);
                        var t2 = Thread.CurrentThread.ManagedThreadId;
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

internal abstract class AwaitOperationDropObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    int runningState; // 0 = stopped, 1 = running

    public AwaitOperationDropObserver(bool configureAwait)
    {
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

    protected override void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);

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
            Interlocked.Exchange(ref runningState, 0);
        }
    }
}

internal abstract class AwaitOperationParallelObserver<T> : Observer<T>
{
    readonly CancellationTokenSource cancellationTokenSource;
    readonly bool configureAwait; // continueOnCapturedContext
    protected readonly object gate = new object(); // need to use gate.

    public AwaitOperationParallelObserver(bool configureAwait)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.configureAwait = configureAwait;
    }

    protected override void OnNextCore(T value)
    {
        StartAsync(value);
    }

    protected override void DisposeCore()
    {
        cancellationTokenSource.Cancel();
    }

    protected abstract ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait);

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
    }
}

// TODO:...
//sealed class SelectAwaitSwitch : Observer<T>
//{
//    readonly Observer<TResult> observer;
//    readonly Func<T, CancellationToken, ValueTask<TResult>> selector;
//    CancellationTokenSource cancellationTokenSource;
//    readonly bool configureAwait; // continueOnCapturedContext

//    int runningState; // 0 = stopped, 1 = running

//    public SelectAwaitSwitch(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
//    {
//        this.observer = observer;
//        this.selector = selector;
//        this.cancellationTokenSource = new CancellationTokenSource();
//        this.configureAwait = configureAwait;
//    }

//    protected override void OnNextCore(T value)
//    {
//        CancellationToken cancellationToken = cancellationTokenSource.Token;
//        if (Interlocked.CompareExchange(ref runningState, 1, 0) == 1)
//        {
//            cancellationTokenSource.Cancel();
//            cancellationTokenSource = new CancellationTokenSource();
//            cancellationToken = cancellationTokenSource.Token;

//        }
//        StartAsync(value, cancellationToken);
//    }

//    protected override void OnErrorResumeCore(Exception error)
//    {
//        observer.OnErrorResume(error);
//    }

//    protected override void OnCompletedCore(Result result)
//    {
//        observer.OnCompleted(result);
//    }

//    protected override void DisposeCore()
//    {
//        cancellationTokenSource.Cancel();
//    }

//    async void StartAsync(T value, CancellationToken token)
//    {
//        try
//        {
//            var v = await selector(value, token).ConfigureAwait(configureAwait);
//            observer.OnNext(v);
//        }
//        catch (Exception ex)
//        {
//            if (ex is OperationCanceledException)
//            {
//                return;
//            }
//            observer.OnErrorResume(ex);
//        }
//        finally
//        {
//            Interlocked.Exchange(ref runningState, 0);
//        }
//    }
//}
