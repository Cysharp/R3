using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> WhereAwait<T>(this Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = false)
    {
        return new WhereAwait<T>(source, predicate, awaitOperations, configureAwait);
    }
}

internal sealed class WhereAwait<T>(Observable<T> source, Func<T, CancellationToken, ValueTask<bool>> predicate, AwaitOperation awaitOperations, bool configureAwait) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        switch (awaitOperations)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new WhereAwaitSequential(observer, predicate, configureAwait));
            case AwaitOperation.Drop:
                return source.Subscribe(new WhereAwaitDrop(observer, predicate, configureAwait));
            case AwaitOperation.Parallel:
                return source.Subscribe(new WhereAwaitParallel(observer, predicate, configureAwait));
            case AwaitOperation.Switch:
                return source.Subscribe(new WhereAwaitSwitch(observer, predicate, configureAwait));
            case AwaitOperation.SequentialParallel:
                return source.Subscribe(new WhereAwaitSequentialParallel(observer, predicate, configureAwait));
            default:
                throw new ArgumentException();
        }
    }

    sealed class WhereAwaitSequential(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait) : AwaitOperationSequentialObserver<T>(configureAwait)
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

    sealed class WhereAwaitDrop(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait) : AwaitOperationDropObserver<T>(configureAwait)
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

    sealed class WhereAwaitParallel(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait) : AwaitOperationParallelObserver<T>(configureAwait)
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

    sealed class WhereAwaitSwitch(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait)
        : AwaitOperationSwitchObserver<T>(configureAwait)
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

    sealed class WhereAwaitSequentialParallel(Observer<T> observer, Func<T, CancellationToken, ValueTask<bool>> predicate, bool configureAwait)
        : AwaitOperationSequentialParallelObserver<T, bool>(configureAwait)
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
}
