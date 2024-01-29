using System.Runtime.CompilerServices;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> SelectAwait<T, TResult>(this Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = false)
    {
        return new SelectAwait<T, TResult>(source, selector, awaitOperations, configureAwait);
    }
}

internal sealed class SelectAwait<T, TResult>(Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperation awaitOperations, bool configureAwait) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        switch (awaitOperations)
        {
            case AwaitOperation.Sequential:
                return source.Subscribe(new SelectAwaitSequential(observer, selector, configureAwait));
            case AwaitOperation.Drop:
                return source.Subscribe(new SelectAwaitDrop(observer, selector, configureAwait));
            case AwaitOperation.Parallel:
                return source.Subscribe(new SelectAwaitParallel(observer, selector, configureAwait));
            case AwaitOperation.Switch:
                return source.Subscribe(new SelectAwaitSwitch(observer, selector, configureAwait));
            case AwaitOperation.SequentialParallel:
                return source.Subscribe(new SelectAwaitSequentialParallel(observer, selector, configureAwait));
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
                observer.OnNext(v);
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
}
