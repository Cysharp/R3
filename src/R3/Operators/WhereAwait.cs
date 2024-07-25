using System.Runtime.CompilerServices;

namespace R3;

public static partial class ObservableExtensions
{
    /// <param name="maxConcurrent">This option is only valid for AwaitOperation.Parallel and AwaitOperation.SequentialParallel. It sets the number of concurrent executions. If set to -1, there is no limit.</param>
    public static Observable<T> WhereAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperation awaitOperation = AwaitOperation.Sequential, bool configureAwait = true, bool cancelOnCompleted = false, int maxConcurrent = -1)
    {
        return new WhereAwait<T>(source, predicate, awaitOperation, configureAwait, cancelOnCompleted, maxConcurrent);
    }
}

internal sealed class WhereAwait<T>(Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperation awaitOperation, bool configureAwait, bool cancelOnCompleted, int maxConcurrent)
    : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        switch (awaitOperation)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new WhereAwaitSequential(observer, predicate, configureAwait, cancelOnCompleted));
            case AwaitOperation.Drop:
                return source.Subscribe(new WhereAwaitDrop(observer, predicate, configureAwait, cancelOnCompleted));
            case AwaitOperation.Switch:
                return source.Subscribe(new WhereAwaitSwitch(observer, predicate, configureAwait, cancelOnCompleted));
            case AwaitOperation.Parallel:
                if (maxConcurrent == -1)
                {
                    return source.Subscribe(new WhereAwaitParallel(observer, predicate, configureAwait, cancelOnCompleted));
                }
                else
                {
                    if (maxConcurrent == 0 || maxConcurrent < -1) throw new ArgumentException("maxConcurrent must be a -1 or greater than 1.");
                    return source.Subscribe(new WhereAwaitParallelConcurrentLimit(observer, predicate, configureAwait, cancelOnCompleted, maxConcurrent));
                }


            case AwaitOperation.SequentialParallel:
                if (maxConcurrent == -1)
                {
                    return source.Subscribe(new WhereAwaitSequentialParallel(observer, predicate, configureAwait, cancelOnCompleted));
                }
                else
                {
                    if (maxConcurrent == 0 || maxConcurrent < -1) throw new ArgumentException("maxConcurrent must be a -1 or greater than 1.");
                    return source.Subscribe(new WhereAwaitSequentialParallelConcurrentLimit(observer, predicate, configureAwait, cancelOnCompleted, maxConcurrent));
                }
            case AwaitOperation.ThrottleFirstLast:
                return source.Subscribe(new WhereAwaitThrottleFirstLast(observer, predicate, configureAwait, cancelOnCompleted));
            default:
                throw new ArgumentException();
        }
    }

    sealed class WhereAwaitSequential(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationSequentialObserver<T>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    observer.OnNext(value);
                }
            }
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

    sealed class WhereAwaitDrop(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationDropObserver<T>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                observer.OnNext(value);
            }
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

    sealed class WhereAwaitParallel(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationParallelObserver<T>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                lock (gate)
                {
                    observer.OnNext(value);
                }
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

    sealed class WhereAwaitSwitch(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationSwitchObserver<T>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                lock (gate)
                {
                    observer.OnNext(value);
                }
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

    sealed class WhereAwaitSequentialParallel(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationSequentialParallelObserver<T, bool>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override ValueTask<bool> OnNextTaskAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            return predicate(value, cancellationToken);
        }

        protected override void PublishOnNext(T value, bool result)
        {
            if (result)
            {
                observer.OnNext(value);
            }
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

    sealed class WhereAwaitParallelConcurrentLimit(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted, int maxConcurrent)
        : AwaitOperationParallelConcurrentLimitObserver<T>(configureAwait, cancelOnCompleted, maxConcurrent)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                lock (gate)
                {
                    observer.OnNext(value);
                }
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

    sealed class WhereAwaitSequentialParallelConcurrentLimit(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted, int maxConcurrent)
        : AwaitOperationSequentialParallelConcurrentLimitObserver<T, bool>(configureAwait, cancelOnCompleted, maxConcurrent)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override ValueTask<bool> OnNextTaskAsyncCore(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            return predicate(value, cancellationToken);
        }

        protected override void PublishOnNext(T value, bool result)
        {
            if (result)
            {
                observer.OnNext(value);
            }
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

    sealed class WhereAwaitThrottleFirstLast(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait, bool cancelOnCompleted)
        : AwaitOperationThrottleFirstLastObserver<T>(configureAwait, cancelOnCompleted)
    {

#if NET6_0_OR_GREATER
        [AsyncMethodBuilderAttribute(typeof(PoolingAsyncValueTaskMethodBuilder))]
#endif
        protected override async ValueTask OnNextAsync(T value, CancellationToken cancellationToken, bool configureAwait)
        {
            if (await predicate(value, cancellationToken).ConfigureAwait(configureAwait))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    observer.OnNext(value);
                }
            }
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
