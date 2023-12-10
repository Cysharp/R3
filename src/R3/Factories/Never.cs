namespace R3
{
    public static partial class EventFactory
    {
        // Never
        public static Event<T> Never<T>()
        {
            return R3.Factories.Never<T>.Instance;
        }

        // NeverComplete
        public static CompletableEvent<TMessage, TComplete> NeverComplete<TMessage, TComplete>()
        {
            return R3.Factories.NeverComplete<TMessage, TComplete>.Instance;
        }
    }
}

namespace R3.Factories
{
    // Never
    internal sealed class Never<T> : Event<T>
    {
        // singleton
        public static readonly Never<T> Instance = new Never<T>();

        Never()
        {
                
        }

        protected override IDisposable SubscribeCore(Subscriber<T> subscriber)
        {
            return Disposable.Empty;
        }
    }


    // NeverComplete
    internal sealed class NeverComplete<TMessage, TComplete> : CompletableEvent<TMessage, TComplete>
    {
        // singleton
        public static readonly NeverComplete<TMessage, TComplete> Instance = new NeverComplete<TMessage, TComplete>();

        NeverComplete()
        {

        }

        protected override IDisposable SubscribeCore(Subscriber<TMessage, TComplete> subscriber)
        {
            return Disposable.Empty;
        }
    }
}
