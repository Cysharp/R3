namespace R3
{
    public static partial class EventFactory
    {
        public static CompletableEvent<int, Unit> Range(int start, int count)
        {
            return new R3.Factories.Range(start, count);
        }
    }
}

namespace R3.Factories
{
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
}
