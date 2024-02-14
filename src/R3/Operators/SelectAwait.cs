using System.Runtime.CompilerServices;

namespace R3;

public static partial class ObservableExtensions
{
    /// <param name="maxConcurrent">This option is only valid for AwaitOperation.Parallel and AwaitOperation.SequentialParallel. It sets the number of concurrent executions. If set to -1, there is no limit.</param>
    public static Observable<TResult> SelectAwait<T, TResult>(this Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = true, int maxConcurrent = -1)
    {
        return new SelectAwait<T, TResult>(source, selector, awaitOperations, configureAwait, maxConcurrent);
    }
}

internal sealed class SelectAwait<T, TResult>(Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperation awaitOperations, bool configureAwait, int maxConcurrent) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        switch (awaitOperations)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new SelectAwaitSequential(observer, selector, configureAwait));
            case AwaitOperation.Drop:
                return source.Subscribe(new SelectAwaitDrop(observer, selector, configureAwait));
            case AwaitOperation.Switch:
                return source.Subscribe(new SelectAwaitSwitch(observer, selector, configureAwait));
            case AwaitOperation.Parallel:
                if (maxConcurrent == -1)
                {
                    return source.Subscribe(new SelectAwaitParallel(observer, selector, configureAwait));
                }
                else
                {
                    if (maxConcurrent == 0 || maxConcurrent < -1) throw new ArgumentException("maxConcurrent must be a -1 or greater than 1.");
                    return source.Subscribe(new SelectAwaitParallelConcurrentLimit(observer, selector, configureAwait, maxConcurrent));
                }
            case AwaitOperation.SequentialParallel:
                if (maxConcurrent == -1)
                {
                    return source.Subscribe(new SelectAwaitSequentialParallel(observer, selector, configureAwait));
                }
                else
                {
                    if (maxConcurrent == 0 || maxConcurrent < -1) throw new ArgumentException("maxConcurrent must be a -1 or greater than 1.");
                    return source.Subscribe(new SelectAwaitSequentialParallelConcurrentLimit(observer, selector, configureAwait, maxConcurrent));
                }
            default:
                throw new ArgumentException();
        }
    }

    sealed class SelectAwaitSequential(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        : AwaitOperationSequentialObserver<T>(configureAwait)
    {
#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            var v = await selector(value, cancellationToken).ConfigureAwait(configureAwait);
            observer.OnNext(v);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void PublishOnCompleted(Result result)
        {
            observer.OnCompleted(result);
        }
    }

    sealed class SelectAwaitDrop(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        : AwaitOperationDropObserver<T>(configureAwait)
    {
#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            var v = await selector(value, cancellationToken).ConfigureAwait(configureAwait);
            observer.OnNext(v);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void PublishOnCompleted(Result result)
        {
            observer.OnCompleted(result);
        }
    }

    sealed class SelectAwaitParallel(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        : AwaitOperationParallelObserver<T>(configureAwait)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            var v = await selector(value, cancellationToken).ConfigureAwait(configureAwait);
            lock (gate)
            {
                observer.OnNext(v);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void PublishOnCompleted(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }
    }


    sealed class SelectAwaitSwitch(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        : AwaitOperationSwitchObserver<T>(configureAwait)
    {
        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            var v = await selector(value, cancellationToken).ConfigureAwait(configureAwait);
            lock (gate)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    observer.OnNext(v);
                }
            }
        }

        protected override void PublishOnCompleted(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }
    }

    sealed class SelectAwaitSequentialParallel(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
        : AwaitOperationSequentialParallelObserver<T, TResult>(configureAwait)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override ValueTask<TResult> OnNextTaskAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            return selector(value, cancellationToken);
        }

        protected override void PublishOnNext(T _, TResult result)
        {
            observer.OnNext(result);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void PublishOnCompleted(Result result)
        {
            observer.OnCompleted(result);
        }
    }

    sealed class SelectAwaitParallelConcurrentLimit(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait, int maxConcurrent)
        : AwaitOperationParallelConcurrentLimitObserver<T>(configureAwait, maxConcurrent)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            var v = await selector(value, cancellationToken).ConfigureAwait(configureAwait);
            lock (gate)
            {
                observer.OnNext(v);
            }
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            lock (gate)
            {
                observer.OnErrorResume(error);
            }
        }

        protected override void PublishOnCompleted(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }
    }

    sealed class SelectAwaitSequentialParallelConcurrentLimit(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait, int maxConcurrent)
        : AwaitOperationSequentialParallelConcurrentLimitObserver<T, TResult>(configureAwait, maxConcurrent)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override ValueTask<TResult> OnNextTaskAsyncCore(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            return selector(value, cancellationToken);
        }

        protected override void PublishOnNext(T _, TResult result)
        {
            observer.OnNext(result);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void PublishOnCompleted(Result result)
        {
            observer.OnCompleted(result);
        }
    }
}
