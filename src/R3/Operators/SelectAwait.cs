using System.Runtime.CompilerServices;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> SelectAwait<T, TResult>(this Observable<T> source, Func<T, CancellationToken, ValueTask<TResult>> selector, AwaitOperation awaitOperations = AwaitOperation.Sequential, bool configureAwait = true)
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
            default:
                throw new ArgumentException();
        }
    }

    sealed class SelectAwaitSequential : AwaitOperationSequentialObserver<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;

        public SelectAwaitSequential(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
            : base(configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
        }

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

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }

    sealed class SelectAwaitDrop : AwaitOperationDropObserver<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;

        public SelectAwaitDrop(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
            : base(configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
        }

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

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }
    }

    sealed class SelectAwaitParallel : AwaitOperationParallelObserver<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, CancellationToken, ValueTask<TResult>> selector;

        public SelectAwaitParallel(Observer<TResult> observer, Func<T, CancellationToken, ValueTask<TResult>> selector, bool configureAwait)
            : base(configureAwait)
        {
            this.observer = observer;
            this.selector = selector;
        }

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

        protected override void OnCompletedCore(Result result)
        {
            lock (gate)
            {
                observer.OnCompleted(result);
            }
        }
    }
}
