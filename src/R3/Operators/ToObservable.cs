namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: more overload?
        public static IObservable<TMessage> ToObservable<TMessage, TComplete>(this Event<TMessage, Unit> source)
        {
            return new ToObservable<TMessage>(source);
        }

        public static IObservable<TMessage> ToObservable<TMessage, TComplete>(this Event<TMessage, Result<Unit>> source)
        {
            return new ToObservableR<TMessage>(source);
        }
    }
}

namespace R3.Operators
{
    internal sealed class ToObservable<TMessage>(Event<TMessage, Unit> source) : IObservable<TMessage>
    {
        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return source.Subscribe(new ObserverToSubscriber(observer));
        }

        sealed class ObserverToSubscriber(IObserver<TMessage> observer) : Subscriber<TMessage, Unit>
        {
            protected override void OnNextCore(TMessage message)
            {
                observer.OnNext(message);
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

    internal sealed class ToObservableR<TMessage>(Event<TMessage, Result<Unit>> source) : IObservable<TMessage>
    {
        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return source.Subscribe(new ObserverToSubscriber(observer));
        }

        sealed class ObserverToSubscriber(IObserver<TMessage> observer) : Subscriber<TMessage, Result<Unit>>
        {
            protected override void OnNextCore(TMessage message)
            {
                observer.OnNext(message);
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

}
