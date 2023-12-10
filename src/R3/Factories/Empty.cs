namespace R3
{
    public static partial class EventFactory
    {
        public static CompletableEvent<TMessage, Unit> Empty<TMessage>()
        {
            return R3.Factories.Empty<TMessage>.Instance;
        }

        public static CompletableEvent<TMessage, Unit> Empty<TMessage>(TimeProvider timeProvider)
        {
            return Empty<TMessage>(TimeSpan.Zero, timeProvider);
        }

        public static CompletableEvent<TMessage, Unit> Empty<TMessage>(TimeSpan dueTime, TimeProvider timeProvider)
        {
            return new EmptyT<TMessage>(dueTime, timeProvider);
        }
    }
}

namespace R3.Factories
{
    internal sealed class Empty<TMessage> : CompletableEvent<TMessage, Unit>
    {
        // singleton
        public static readonly Empty<TMessage> Instance = new Empty<TMessage>();

        protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
        {
            subscriber.OnCompleted(default);
            return Disposable.Empty;
        }

        Empty()
        {

        }
    }

    internal sealed class EmptyT<TMessage>(TimeSpan dueTime, TimeProvider timeProvider) : CompletableEvent<TMessage, Unit>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage, Unit> subscriber)
        {
            var method = new _Empty(subscriber);
            method.Timer = timeProvider.CreateStoppedTimer(_Empty.timerCallback, method);
            method.Timer.InvokeOnce(dueTime);
            return method;
        }

        sealed class _Empty(Subscriber<TMessage, Unit> subscriber) : IDisposable
        {
            public static readonly TimerCallback timerCallback = NextTick;

            readonly Subscriber<TMessage, Unit> subscriber = subscriber;

            public ITimer? Timer { get; set; }

            static void NextTick(object? state)
            {
                var self = (_Empty)state!;
                try
                {
                    self.subscriber.OnCompleted();
                }
                finally
                {
                    self.Dispose();
                }
            }

            public void Dispose()
            {
                Timer?.Dispose();
                Timer = null;
            }
        }
    }
}
