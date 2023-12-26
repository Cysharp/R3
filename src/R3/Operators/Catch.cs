namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<T> Catch<T>(this Observable<T> source, Observable<T> second)
    {
        return new Catch<T>(source, second);
    }

    public static Observable<T> Catch<T, TException>(this Observable<T> source, Func<TException, Observable<T>> errorHandler)
    {
        return new Catch<T, TException>(source, errorHandler);
    }
}

internal sealed class Catch<T>(Observable<T> source, Observable<T> second) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _Catch(observer, second).Run(source);
    }

    sealed class _Catch(Observer<T> observer, Observable<T> second) : IDisposable
    {
        readonly Observer<T> observer = observer;
        readonly Observable<T> second = second;
        SingleAssignmentDisposableCore firstSubscription;
        SingleAssignmentDisposableCore secondSubscription;

        public IDisposable Run(Observable<T> source)
        {
            return source.Subscribe(new FirstObserver(this));
        }

        public void Dispose()
        {
            firstSubscription.Dispose();
            secondSubscription.Dispose();
        }

        internal sealed class FirstObserver(_Catch parent) : Observer<T>
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
                if (result.IsFailure)
                {
                    parent.secondSubscription.Disposable = parent.second.Subscribe(new SecondObserver(parent));
                }
                else
                {
                    parent.observer.OnCompleted(result);
                }
            }
        }

        internal sealed class SecondObserver(_Catch parent) : Observer<T>
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

internal sealed class Catch<T, TException>(Observable<T> source, Func<TException, Observable<T>> errorHandler) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _Catch(observer, errorHandler).Run(source);
    }

    sealed class _Catch(Observer<T> observer, Func<TException, Observable<T>> errorHandler) : IDisposable
    {
        readonly Observer<T> observer = observer;
        readonly Func<TException, Observable<T>> errorHandler = errorHandler;
        SingleAssignmentDisposableCore firstSubscription;
        SingleAssignmentDisposableCore secondSubscription;

        public IDisposable Run(Observable<T> source)
        {
            return source.Subscribe(new FirstObserver(this));
        }

        public void Dispose()
        {
            firstSubscription.Dispose();
            secondSubscription.Dispose();
        }

        internal sealed class FirstObserver(_Catch parent) : Observer<T>
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
                if (result.IsFailure && result.Exception is TException error)
                {
                    parent.secondSubscription.Disposable = parent.errorHandler(error).Subscribe(new SecondObserver(parent));
                }
                else
                {
                    parent.observer.OnCompleted(result);
                }
            }
        }

        internal sealed class SecondObserver(_Catch parent) : Observer<T>
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
