using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = false)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), Stubs.HandleResult, awaitOperations, configureAwait);
    }

    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Result> onCompleted, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = false)
    {
        return SubscribeAwait(source, onNextAsync, ObservableSystem.GetUnhandledExceptionHandler(), onCompleted, awaitOperations, configureAwait);
    }

    public static IDisposable SubscribeAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = false)
    {
        switch (awaitOperations)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new SubscribeAwaitSequential<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperation.Drop:
                return source.Subscribe(new SubscribeAwaitDrop<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            case AwaitOperation.Parallel:
                return source.Subscribe(new SubscribeAwaitParallel<T>(onNextAsync, onErrorResume, onCompleted, configureAwait));
            default:
                throw new ArgumentException();
        }
    }
}

internal sealed class SubscribeAwaitSequential<T> : AwaitOperationSequentialObserver<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;

    public SubscribeAwaitSequential(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
        : base(configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
    }

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

internal sealed class SubscribeAwaitDrop<T> : AwaitOperationDropObserver<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;

    public SubscribeAwaitDrop(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
        : base(configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
    }

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

sealed class SubscribeAwaitParallel<T> : AwaitOperationParallelObserver<T>
{
    readonly Func<T, CancellationToken, ValueTask> onNextAsync;
    readonly Action<Exception> onErrorResume;
    readonly Action<Result> onCompleted;

    public SubscribeAwaitParallel(Func<T, CancellationToken, ValueTask> onNextAsync, Action<Exception> onErrorResume, Action<Result> onCompleted, bool configureAwait)
        : base(configureAwait)
    {
        this.onNextAsync = onNextAsync;
        this.onErrorResume = onErrorResume;
        this.onCompleted = onCompleted;
    }

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
