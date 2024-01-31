using System.Threading;

namespace R3.Tests.OperatorTests;

public class DebounceThrottleFirstThrottleLastTest
{
    // Debounce
    [Fact]
    public void Debounce()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.Debounce(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));

        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        list.AssertEqual([10000]);

        publisher.OnNext(2);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        list.AssertEqual([10000]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));

        list.AssertEqual([10000, 2]);

        publisher.OnNext(3);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([10000, 2]);

        publisher.OnCompleted();

        list.AssertEqual([10000, 2, 3]);
    }

    // ThrottleFirst
    [Fact]
    public void ThrottleFirst()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleFirst(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([1]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        publisher.OnNext(2);
        list.AssertEqual([1, 2]);
        publisher.OnNext(20);
        publisher.OnNext(200);

        timeProvider.Advance(TimeSpan.FromSeconds(3));
        list.AssertEqual([1, 2]);
        publisher.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    // Sample
    [Fact]
    public void Sample()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleLast(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([10000]);
        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        timeProvider.Advance(TimeSpan.FromSeconds(3));
        list.AssertEqual([10000, 200]);
        publisher.OnNext(3);

        publisher.OnCompleted();

        list.AssertEqual([10000, 200]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void DebounceFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.DebounceFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        frameProvider.Advance(1);

        list.AssertEqual([]);

        frameProvider.Advance(2);

        list.AssertEqual([10000]);

        publisher.OnNext(2);

        frameProvider.Advance(2);
        list.AssertEqual([10000]);

        frameProvider.Advance(1);

        list.AssertEqual([10000, 2]);

        publisher.OnNext(3);

        frameProvider.Advance(1);
        list.AssertEqual([10000, 2]);

        publisher.OnCompleted();

        list.AssertEqual([10000, 2, 3]);
    }

    [Fact]
    public void ThrottleFirstFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleFirstFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([1]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1]);

        frameProvider.Advance(1);
        publisher.OnNext(2);
        list.AssertEqual([1, 2]);
        publisher.OnNext(20);
        publisher.OnNext(200);

        frameProvider.Advance(3);
        publisher.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void SampleFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleLastFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        frameProvider.Advance(1);
        list.AssertEqual([10000]);
        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        frameProvider.Advance(3);
        list.AssertEqual([10000, 200]);
        publisher.OnNext(3);

        publisher.OnCompleted();

        list.AssertEqual([10000, 200]);
        list.AssertIsCompleted();
    }

    // + Observable/Async sampler
    [Fact]
    public void ThrottleFirstAsyncSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var fakeTime = new FakeTimeProvider();
        var list = publisher.ThrottleFirst(async (x, ct) =>
        {
            await fakeTime.Delay(TimeSpan.FromSeconds(x), ct);
        }).ToLiveList();

        publisher.OnNext(1); // gate close
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([1]);

        fakeTime.Advance(1); // gate open

        publisher.OnNext(5);
        publisher.OnNext(6);
        publisher.OnNext(7);

        list.AssertEqual([1, 5]);

        fakeTime.Advance(4);
        publisher.OnNext(8);

        list.AssertEqual([1, 5]);

        fakeTime.Advance(1);
        list.AssertEqual([1, 5]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ThrottleLastAsyncSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var fakeTime = new FakeTimeProvider();
        var list = publisher.ThrottleLast(async (x, ct) =>
        {
            await fakeTime.Delay(TimeSpan.FromSeconds(x), ct);
        }).ToLiveList();

        publisher.OnNext(1); // gate close
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([]);

        fakeTime.Advance(1); // gate open
        list.AssertEqual([3]);

        publisher.OnNext(5);
        publisher.OnNext(6);
        publisher.OnNext(7);

        list.AssertEqual([3]);

        fakeTime.Advance(4);
        publisher.OnNext(8);

        list.AssertEqual([3]);

        fakeTime.Advance(1);
        list.AssertEqual([3, 8]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ThrottleFirstObservableSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var sampler = new Subject<Unit>();
        var list = publisher.ThrottleFirst(sampler).ToLiveList();

        publisher.OnNext(1); // gate close
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([1]);

        sampler.OnNext(Unit.Default);

        publisher.OnNext(5);
        list.AssertEqual([1, 5]);
        publisher.OnNext(6);
        publisher.OnNext(7);
        list.AssertEqual([1, 5]);

        sampler.OnNext(Unit.Default);

        publisher.OnNext(8);
        list.AssertEqual([1, 5, 8]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([1, 5, 8]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ThrottleLastObservableSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var sampler = new Subject<Unit>();
        var list = publisher.ThrottleLast(sampler).ToLiveList();

        publisher.OnNext(1); // gate close
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([3]);

        publisher.OnNext(5);
        list.AssertEqual([3]);
        publisher.OnNext(6);
        publisher.OnNext(7);
        list.AssertEqual([3]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([3, 7]);

        publisher.OnNext(8);
        list.AssertEqual([3, 7]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([3, 7, 8]);
        sampler.OnNext(Unit.Default);
        sampler.OnNext(Unit.Default);
        list.AssertEqual([3, 7, 8]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void DebounceSelector()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var fakeTime = new FakeTimeProvider();
        var list = publisher.Debounce(async (x, ct) =>
        {
            try
            {
                // await fakeTime.Delay(TimeSpan.FromSeconds(x), ct);
                await Task.Delay(TimeSpan.FromSeconds(x), fakeTime, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }).ToLiveList();



        publisher.OnNext(1);   // cancel
        publisher.OnNext(10);  // cancel
        publisher.OnNext(100); // wait 100...
        list.AssertEqual([]);

        fakeTime.Advance(100);
        list.AssertEqual([100]);

        publisher.OnNext(1000);  // cancel
        publisher.OnNext(10000); // wait 10000
        list.AssertEqual([100]);

        fakeTime.Advance(10000);

        list.AssertEqual([100, 10000]);

        publisher.OnNext(5);
        fakeTime.Advance(5);

        publisher.OnNext(6);
        fakeTime.Advance(6);

        publisher.OnCompleted();

        list.AssertEqual([100, 10000, 5, 6]);

        list.AssertIsCompleted();
    }
}
