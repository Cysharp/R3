namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Concat<T>(this Observable<T> source, Observable<T> second)
    {
        return new Concat<T>(source, second);
    }
}

internal sealed class Concat<T>(Observable<T> source, Observable<T> second) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Concat(observer, second));
    }

    sealed class _Concat(Observer<T> observer, Observable<T> second) : Observer<T>
    {
        readonly Observer<T> observer = observer;

        SingleAssignmentDisposableCore secondSubscription;

        protected override bool AutoDisposeOnCompleted => false;

        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                try
                {
                    observer.OnCompleted(result);
                }
                finally
                {
                    Dispose();
                }
            }
            else
            {
                secondSubscription.Disposable = second.Subscribe(new SecondObserver(this));
            }
        }

        protected override void DisposeCore()
        {
            secondSubscription.Dispose();
        }

        internal sealed class SecondObserver(_Concat parent) : Observer<T>
        {
            protected override void OnNextCore(T value)
            {
                parent.observer.OnNext(value);
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                parent.observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                parent.observer.OnCompleted(result);
            }

            protected override void DisposeCore()
            {
                parent.Dispose();
            }
        }
    }
}
