namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<Notification<T>> Materialize<T>(this Observable<T> source)
    {
        return new Materialize<T>(source);
    }

    public static Observable<T> Dematerialize<T>(this Observable<Notification<T>> source)
    {
        return new Dematerialize<T>(source);
    }
}

internal sealed class Materialize<T>(Observable<T> source) : Observable<Notification<T>>
{
    protected override IDisposable SubscribeCore(Observer<Notification<T>> observer)
    {
        return source.Subscribe(new _Materialize(observer));
    }

    sealed class _Materialize(Observer<Notification<T>> observer) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(new(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnNext(new(error));
        }

        protected override void OnCompletedCore(Result result)
        {
            try
            {
                observer.OnNext(new(result));
            }
            finally
            {
                observer.OnCompleted();
            }
        }
    }
}

internal sealed class Dematerialize<T>(Observable<Notification<T>> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return source.Subscribe(new _Dematerialize(observer));
    }

    sealed class _Dematerialize(Observer<T> observer) : Observer<Notification<T>>
    {
        protected override void OnNextCore(Notification<T> value)
        {
            switch (value.Kind)
            {
                case NotificationKind.OnNext:
                    observer.OnNext(value.Value!);
                    break;
                case NotificationKind.OnErrorResume:
                    OnErrorResume(value.Error!);
                    break;
                case NotificationKind.OnCompleted:
                    OnCompleted(value.Result!);
                    break;
                default:
                    break;
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

