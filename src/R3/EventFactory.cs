using R3.Internal;

namespace R3;

public static partial class EventFactory
{
    public static CompletableEvent<int, Unit> Range(int start, int count)
    {
        return new Range(start, count);
    }

    public static CompletableEvent<TMessage, Unit> ToEvent<TMessage>(this IEnumerable<TMessage> source)
    {
        return new EnumerableToEvent<TMessage>(source);
    }

    public static CompletableEvent<Unit, Unit> Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        return new Timer(dueTime, timeProvider);
    }
}

internal sealed class Range : CompletableEvent<int, Unit>
{
    readonly int start;
    readonly int count;

    public Range(int start, int count)
    {
        this.start = start;
        this.count = count;
    }

    protected override IDisposable SubscribeCore(Subscriber<int, Unit> subscriber)
    {
        for (int i = 0; i < count; i++)
        {
            subscriber.OnNext(start + i);
        }
        subscriber.OnCompleted(default);
        return Disposable.Empty;
    }
}

internal class EnumerableToEvent<TMessage>(IEnumerable<TMessage> source) : CompletableEvent<TMessage, Unit>
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

internal sealed class Timer : CompletableEvent<Unit, Unit>
{
    readonly TimeSpan dueTime;
    readonly TimeProvider timeProvider;

    public Timer(TimeSpan dueTime, TimeProvider timeProvider)
    {
        this.dueTime = dueTime;
        this.timeProvider = timeProvider;
    }

    protected override IDisposable SubscribeCore(Subscriber<Unit, Unit> subscriber)
    {
        var method = new _Timer(subscriber);
        method.Timer = timeProvider.CreateStoppedTimer(_Timer.timerCallback, method);
        method.Timer.InvokeOnce(dueTime);
        return method;
    }

    sealed class _Timer(Subscriber<Unit, Unit> subscriber) : IDisposable
    {
        public static readonly TimerCallback timerCallback = NextTick;

        Subscriber<Unit, Unit> subscriber = subscriber;

        public ITimer? Timer { get; set; }

        static void NextTick(object? state)
        {
            var self = (_Timer)state!;
            try
            {
                self.subscriber.OnNext(default);
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
