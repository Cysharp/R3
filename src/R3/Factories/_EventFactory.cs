
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace R3
{
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


        //public static Event<Unit> EveryUpdate(FrameProvider frameProvider)
        //{
        //    return new R3.Factories.EveryUpdate(frameProvider);
        //}

        //public static CompletableEvent<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken)
        //{
        //    return new R3.Factories.EveryUpdate(frameProvider);
        //}
    }
}

namespace R3.Factories
{
    //internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken) : Event<Unit>
    //{
    //    protected override IDisposable SubscribeCore(Subscriber<Unit> subscriber)
    //    {
    //        var runner = new EveryUpdateRunnerWorkItem(subscriber, cancellationToken);
    //        frameProvider.Register(runner);
    //        return runner;
    //    }

    //    class EveryUpdateRunnerWorkItem(Subscriber<Unit> subscriber, CancellationToken cancellationToken) : IFrameRunnerWorkItem, IDisposable
    //    {
    //        bool isDisposed;

    //        public bool MoveNext(long frameCount)
    //        {
    //            if (isDisposed || cancellationToken.IsCancellationRequested)
    //            {
    //                return false;
    //            }

    //            subscriber.OnNext(default);
    //            return true;
    //        }

    //        public void Dispose()
    //        {
    //            isDisposed = true;
    //        }
    //    }
    //}

    //internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken) : Event<Unit>
    //{
    //    protected override IDisposable SubscribeCore(Subscriber<Unit> subscriber)
    //    {
    //        var runner = new EveryUpdateRunnerWorkItem(subscriber, cancellationToken);
    //        frameProvider.Register(runner);
    //        return runner;
    //    }

    //    class EveryUpdateRunnerWorkItem(Subscriber<Unit> subscriber, CancellationToken cancellationToken) : IFrameRunnerWorkItem, IDisposable
    //    {
    //        bool isDisposed;

    //        public bool MoveNext(long frameCount)
    //        {
    //            if (isDisposed || cancellationToken.IsCancellationRequested)
    //            {
    //                return false;
    //            }

    //            subscriber.OnNext(default);
    //            return true;
    //        }

    //        public void Dispose()
    //        {
    //            isDisposed = true;
    //        }
    //    }
    //}
}
