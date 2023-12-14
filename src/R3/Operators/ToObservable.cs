namespace R3;

public static partial class EventExtensions
{
    // TODO: more overload?
    public static IObservable<T> ToObservable<T>(this Event<TMessage> source)
    {
        return new ToObservable<T>(source);
    }

    public static IObservable<T> ToObservable<T>(this Event<TMessage> source)
    {
        return new ToObservableR<T>(source);
    }
}

internal sealed class ToObservable<T>(Event<TMessage> source) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
    {
        return source.Subscribe(new ObserverToSubscriber(observer));
    }

    sealed class ObserverToSubscriber(IObserver<T> observer) : Subscriber<TMessage>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnError(error);
        }

        protected override void OnCompletedCore(Unit complete)
        {
            observer.OnCompleted();
        }
    }
}

internal sealed class ToObservableR<T>(Event<TMessage> source) : IObservable<T>
{
    public IDisposable Subscribe(IObserver<T> observer)
    {
        return source.Subscribe(new ObserverToSubscriber(observer));
    }

    sealed class ObserverToSubscriber(IObserver<T> observer) : Subscriber<TMessage>
    {
        protected override void OnNextCore(T value)
        {
            observer.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnError(error);
        }

        protected override void OnCompletedCore(Result<Unit> complete)
        {
            if (complete.IsFailure)
            {
                observer.OnError(complete.Exception);
            }
            else
            {
                observer.OnCompleted();
            }
        }
    }
}

