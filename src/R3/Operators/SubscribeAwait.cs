using System;
using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    /// <param name="maxConcurrent">This option is only valid for AwaitOperation.Parallel and AwaitOperation.SequentialParallel. It sets the number of concurrent executions. If set to -1, there is no limit.</param>
    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = true, int maxConcurrent = -1)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), Stubs.HandleResult, awaitOperations, configureAwait, maxConcurrent);
    }

    /// <param name="maxConcurrent">This option is only valid for AwaitOperation.Parallel and AwaitOperation.SequentialParallel. It sets the number of concurrent executions. If set to -1, there is no limit.</param>
    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Result> onCompleted, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = true, int maxConcurrent = -1)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), onCompleted, awaitOperations, configureAwait, maxConcurrent);
    }

    /// <param name="maxConcurrent">This option is only valid for AwaitOperation.Parallel and AwaitOperation.SequentialParallel. It sets the number of concurrent executions. If set to -1, there is no limit.</param>
    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = true, int maxConcurrent = -1)
    {
        switch (awaitOperations)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new SubscribeAwaitSequential<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperation.Drop:
                return source.Subscribe(new SubscribeAwaitDrop<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperation.Parallel:
                if (maxConcurrent == -1)
                {
                    return source.Subscribe(new SubscribeAwaitParallel<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
                }
                else
                {
                    if (maxConcurrent == 0 || maxConcurrent < -1) throw new ArgumentException("maxConcurrent must be a -1 or greater than 1.");
                    return source.Subscribe(new SubscribeAwaitParallelConcurrentLimit<T>(onNextAsync, onErrorResume, onCompleted, configureAwait, maxConcurrent));
                }
            case AwaitOperation.Switch:
                return source.Subscribe(new SubscribeAwaitSwitch<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperation.SequentialParallel:
                throw new ArgumentException("SubscribeAwait does not support SequentialParallel. Use Sequential for sequential operation, use parallel for parallel operation instead.");
            case AwaitOperation.Latest:
                return source.Subscribe(new SubscribeAwaitLatest<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            default:
                throw new ArgumentException();
        }
    }
}

internal sealed class SubscribeAwaitSequential<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    : AwaitOperationSequentialObserver<T>(configureAwait)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    protected override void PublishOnCompleted(Result result)
    {
        onCompleted(result);
    }
}

internal sealed class SubscribeAwaitDrop<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    : AwaitOperationDropObserver<T>(configureAwait)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    protected override void PublishOnCompleted(Result result)
    {
        onCompleted(result);
    }
}

sealed class SubscribeAwaitParallel<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    : AwaitOperationParallelObserver<T>(configureAwait)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        lock (gate)
        {
            onErrorResume(error);
        }
    }

    protected override void PublishOnCompleted(Result result)
    {
        lock (gate)
        {
            onCompleted(result);
        }
    }
}

sealed class SubscribeAwaitSwitch<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    : AwaitOperationSwitchObserver<T>(configureAwait)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        lock (gate)
        {
            onErrorResume(error);
        }
    }

    protected override void PublishOnCompleted(Result result)
    {
        lock (gate)
        {
            onCompleted(result);
        }
    }
}

sealed class SubscribeAwaitParallelConcurrentLimit<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait, int maxConcurrent)
    : AwaitOperationParallelConcurrentLimitObserver<T>(configureAwait, maxConcurrent)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        lock (gate)
        {
            onErrorResume(error);
        }
    }

    protected override void PublishOnCompleted(Result result)
    {
        lock (gate)
        {
            onCompleted(result);
        }
    }
}

internal sealed class SubscribeAwaitLatest<T>(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
    : AwaitOperationLatestObserver<T>(configureAwait)
{
    protected override ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
    {
        return onNextAsync(value, cancellationToken);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        onErrorResume(error);
    }

    protected override void PublishOnCompleted(Result result)
    {
        onCompleted(result);
    }
}
