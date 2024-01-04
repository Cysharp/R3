namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> WithLatestFrom<TFirst, TSecond, TResult>(this Observable<TFirst> first, Observable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        return new WithLatestFrom<TFirst, TSecond, TResult>(first, second, resultSelector);
    }
}

internal sealed class WithLatestFrom<TFirst, TSecond, TResult>(Observable<TFirst> first, Observable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        var firstObserver = new WithLatestFromFirstObserver(observer, resultSelector);
        var secondObserver = new WithLatestFromSecondObserver(firstObserver);
        firstObserver.secondDisposable.Disposable = secondObserver;

        // important: subscribe second first.
        second.Subscribe(secondObserver);
        try
        {
            first.Subscribe(firstObserver);
        }
        catch
        {
            secondObserver.Dispose();
            throw;
        }

        return firstObserver; // return first(first has secondDisposable)
    }

    sealed class WithLatestFromFirstObserver(Observer<TResult> observer, Func<TFirst, TSecond, TResult> resultSelector) : Observer<TFirst>
    {
        public Observer<TResult> observer = observer;
        public bool hasSecondValue;
        public TSecond? secondValue;
        public SingleAssignmentDisposableCore secondDisposable;

        protected override void OnNextCore(TFirst value)
        {
            // drop when second value is not available
            if (hasSecondValue)
            {
                var result = resultSelector(value, secondValue!);
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

        protected override void DisposeCore()
        {
            secondDisposable.Dispose();
        }
    }

    sealed class WithLatestFromSecondObserver(WithLatestFromFirstObserver left) : Observer<TSecond>
    {
        protected override void OnNextCore(TSecond value)
        {
            left.secondValue = value;
            Interlocked.MemoryBarrier();
            left.hasSecondValue = true;
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            left.observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            // only propagate failure
            if (result.IsFailure)
            {
                left.observer.OnCompleted(result);
            }
        }
    }
}
