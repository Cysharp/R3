using R2.Internal;

namespace R2;

public static partial class EventFactory
{
    public static ICompletableEvent<TMessage, Unit> ToEvent<TMessage>(this IEnumerable<TMessage> source)
    {
        return new EnumerableToEvent<TMessage>(source);
    }

    public static ICompletableEvent<long, Unit> Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Timer(dueTime, timeProvider);
    }
}

internal class EnumerableToEvent<TMessage>(IEnumerable<TMessage> source) : ICompletableEvent<TMessage, Unit>
{
    public IDisposable Subscribe(ISubscriber<TMessage, Unit> subscriber)
    {
        foreach (var message in source)
        {
            subscriber.OnNext(message);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal class Timer : ICompletableEvent<long, Unit>
{
    readonly TimeSpan dueTime;
    readonly TimeProvider timeProvider;

    public Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        this.dueTime = dueTime;
        this.timeProvider = timeProvider;
    }

    public IDisposable Subscribe(ISubscriber<long, Unit> subscriber)
    {
        var method = new _Timer(subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_Timer.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Timer(ISubscriber<long, Unit> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        ISubscriber<long, Unit> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Timer)state!;
            try
            {
                self.subscriber.OnNext(0);
                self.subscriber.OnCompleted(Unit.Default);
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