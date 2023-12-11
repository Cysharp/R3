namespace R3
{
    public static partial class EventFactory
    {
        // TODO: this is working space, will remove this file after complete.

        // TODO: Range, Repeat, Defer, DeferAsync, FromAsync, FromAsyncPattern, FromEvent, FromEventPattern, Start, Using, Create
        // Timer, Interval, TimerFrame, IntervalFrame, ToObservable(ToEvent)

        // TODO: Convert
        // ToArray
        // ToAsync
        // ToDictionary
        // ToHashSet
        // ToEnumerable
        // ToEvent
        // ToEventPattern
        // ToList
        // ToLookup



        // AsObservable
        // AsSingleUnitObservable
        // AsUnitObservable
        // AsNeverComplete

        // Repeat
        public static CompletableEvent<TMessage, Unit> Repeat<TMessage>(TMessage value)
        {
            return new ImmediateScheduleReturn<TMessage, Unit>(value, default); // immediate
        }
    }
}

namespace R3.Factories
{
}
