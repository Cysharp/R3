namespace R3
{
    public static partial class EventFactory
    {
        public static CompletableEvent<TMessage, Unit> ToEvent<TMessage>(this IEnumerable<TMessage> source)
        {
            return new ToEvent<TMessage>(source);
        }
    }
}

namespace R3.Factories
{
    internal class ToEvent<TMessage>(IEnumerable<TMessage> source) : CompletableEvent<TMessage, Unit>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
        {
            foreach (var message in source)
            {
                subscriber.OnNext(message);
            }
            subscriber.OnCompleted(default);
            return Disposable.Empty;
        }
    }
}
