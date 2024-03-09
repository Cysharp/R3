namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> ScanSelect<TSource, TResult>(this Observable<TSource> source, Func<TSource, TSource, TResult> selector)
    {
        if (source is Where<TSource> where)
        {
            // Optimize for WhereScanSelect
            return new WhereScanSelect<TSource, TResult>(where.source, selector, where.predicate);
        }

        return new ScanSelect<TSource, TResult>(source, selector);
    }

    // TState

    public static Observable<TResult> ScanSelect<TSource, TResult, TState>(this Observable<TSource> source, TState state, Func<TSource, TSource, TState, TResult> selector)
    {
        return new ScanSelect<TSource, TResult, TState>(source, selector, state);
    }
}

internal sealed class ScanSelect<T, TResult>(Observable<T> source, Func<T, T, TResult> selector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _ScanSelect(observer, selector));
    }

    sealed class _ScanSelect : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, T, TResult> selector;
        T accumulation;

        public _ScanSelect(Observer<TResult> observer, Func<T, T, TResult> selector)
        {
            this.observer = observer;
            this.accumulation = default!;
            this.selector = selector;
        }

        protected override void OnNextCore(T value)
        {
            var result = selector(accumulation, value);
            accumulation = value;
            observer.OnNext(result);
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
}

internal sealed class ScanSelect<T, TResult, TState>(Observable<T> source, Func<T, T, TState, TResult> selector, TState state) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _ScanSelect(observer, selector, state));
    }

    sealed class _ScanSelect : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, T, TState, TResult> selector;
        T accumulation;
        TState state;

        public _ScanSelect(Observer<TResult> observer, Func<T, T, TState, TResult> selector, TState state)
        {
            this.observer = observer;
            this.accumulation = default!;
            this.selector = selector;
            this.state = state;
        }

        protected override void OnNextCore(T value)
        {
            var result = selector(accumulation, value, state);
            accumulation = value;
            observer.OnNext(result);
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
}

internal sealed class WhereScanSelect<T, TResult>(Observable<T> source, Func<T, T, TResult> selector, Func<T, bool> predicate) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        return source.Subscribe(new _WhereScanSelect(observer, selector, predicate));
    }

    sealed class _WhereScanSelect : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, T, TResult> selector;
        readonly Func<T, bool> predicate;
        T accumulation;

        public _WhereScanSelect(Observer<TResult> observer, Func<T, T, TResult> selector, Func<T, bool> predicate)
        {
            this.observer = observer;
            this.accumulation = default!;
            this.selector = selector;
            this.predicate = predicate;
        }

        protected override void OnNextCore(T value)
        {
            if (predicate(value))
            {
                var result = selector(accumulation, value);
                accumulation = value;
                observer.OnNext(result);
            }
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
}
