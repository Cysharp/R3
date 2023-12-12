namespace R3
{
    public static partial class EventExtensions
    {
        // TODO: more overload
        public static IObservable<TMessage> ToObservable<TMessage>(this Event<TMessage> source)
        {
            return new ToObservable<TMessage>(source);
        }
    }
}

namespace R3.Operators
{
    internal sealed class ToObservable<TMessage>(Event<TMessage> source) : IObservable<TMessage>
    {
        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return source.Subscribe(new ObserverToSubscriber<TMessage>(observer));
        }
    }

    internal sealed class ObserverToSubscriber<TMessage>(IObserver<TMessage> observer) : Subscriber<TMessage>
    {
        protected override void OnNextCore(TMessage message)
        {
            observer.OnNext(message);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }
    }
}
