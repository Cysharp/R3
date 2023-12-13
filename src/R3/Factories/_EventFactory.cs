
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace R3;

public static partial class Event
{
    // TODO: this is working space, will remove this file after complete.

    // TODO: Defer, DeferAsync, FromAsync, FromAsyncPattern, FromEvent, FromEventPattern, Start, Using, Create
    // Timer, Interval, TimerFrame, IntervalFrame, ToObservable(ToEvent)



    // ToAsyncEnumerable?
    // ToEvent
    // ToEventPattern



    // AsObservable
    // AsSingleUnitObservable

    // AsUnitObservable
    // AsUnitComplete
    // AsNeverComplete

    // TODO: use SystemDefault

    public static Event<Unit, Unit> EveryUpdate()
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, CancellationToken.None);
    }

    public static Event<Unit, Unit> EveryUpdate(CancellationToken cancellationToken)
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, cancellationToken);
    }

    public static Event<Unit, Unit> EveryUpdate(FrameProvider frameProvider)
    {
        return new EveryUpdate(frameProvider, CancellationToken.None);
    }

    public static Event<Unit, Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken)
    {
        return new EveryUpdate(frameProvider, cancellationToken);
    }
}



internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken) : Event<Unit, Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<Unit, Unit> subscriber)
    {
        var runner = new EveryUpdateRunnerWorkItem(subscriber, cancellationToken);
        frameProvider.Register(runner);
        return runner;
    }

    class EveryUpdateRunnerWorkItem(Subscriber<Unit, Unit> subscriber, CancellationToken cancellationToken) : IFrameRunnerWorkItem, IDisposable
    {
        bool isDisposed;

        public bool MoveNext(long frameCount)
        {
            if (isDisposed)
            {
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                subscriber.OnCompleted();
                Dispose();
                return false;
            }

            subscriber.OnNext(default);
            return true;
        }

        public void Dispose()
        {
            isDisposed = true;
        }
    }
}
