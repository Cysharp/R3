using R2.Internal;

namespace R2;

public static partial class EventFactory
{
    public static ICompletableEvent<TMessage, Unit> Return<TMessage>(TMessage value, TimeProvider timeProvider)
    {
        return Return(value, TimeSpan.Zero, timeProvider);
    }

    public static ICompletableEvent<TMessage, Unit> Return<TMessage>(TMessage value, TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Return<TMessage>(value, TimeSpan.Zero, timeProvider);
    }
}

internal class Return<TMessage>(TMessage value, TimeSpan dueTime, TimeProvider timeProvider) : ICompletableEvent<TMessage, Unit>
{
    public IDisposable Subscribe(ISubscriber<TMessage, Unit> subscriber)
    {
        var method = new _Return(value, subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_Return.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Return(TMessage value, ISubscriber<TMessage, Unit> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        readonly TMessage value = value;
        readonly ISubscriber<TMessage, Unit> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Return)state!;
            try
            {
                self.subscriber.OnNext(self.value);
                self.subscriber.OnCompleted(default);
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
