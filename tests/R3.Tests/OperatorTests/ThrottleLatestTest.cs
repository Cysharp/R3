namespace R3.Tests.OperatorTests;

public class ThrottleLatestTest
{
    // ThrottleLatest(TimeSpan)
    [Fact]
    public void ThrottleLatest()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleLatest(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

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
        list.AssertEqual([1, 10000]);
        publisher.OnNext(2);
        list.AssertEqual([1, 10000, 2]);
        publisher.OnNext(20);
        publisher.OnNext(200);

        timeProvider.Advance(TimeSpan.FromSeconds(3));
        list.AssertEqual([1, 10000, 2, 200]);
        publisher.OnNext(3);
        list.AssertEqual([1, 10000, 2, 200, 3]);

        publisher.OnCompleted();

        list.AssertEqual([1, 10000, 2, 200, 3]);
        list.AssertIsCompleted();
    }

    // ThrottleLatest(async)
    [Fact]
    public void ThrottleLatestAsyncSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var fakeTime = new FakeTimeProvider();
        var list = publisher.ThrottleLatest(async (x, ct) =>
        {
            await fakeTime.Delay(TimeSpan.FromSeconds(x), ct);
        }).ToLiveList();

        publisher.OnNext(1); // gate close
        list.AssertEqual([1]);
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([1]);

        fakeTime.Advance(1); // gate open
        list.AssertEqual([1, 3]);

        publisher.OnNext(5);
        list.AssertEqual([1, 3, 5]);

        publisher.OnNext(6);
        publisher.OnNext(7);

        fakeTime.Advance(4);
        list.AssertEqual([1, 3, 5]);

        publisher.OnNext(8);

        list.AssertEqual([1, 3, 5]);

        fakeTime.Advance(1);
        list.AssertEqual([1, 3, 5, 8]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    // ThrottleLatest(Observable)
    [Fact]
    public void ThrottleLatestObservableSampler()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var sampler = new Subject<Unit>();
        var list = publisher.ThrottleLatest(sampler).ToLiveList();

        publisher.OnNext(1); // gate close
        list.AssertEqual([1]);
        publisher.OnNext(2);
        publisher.OnNext(3);

        list.AssertEqual([1]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([1, 3]);

        publisher.OnNext(5);
        list.AssertEqual([1, 3, 5]);
        publisher.OnNext(6);
        publisher.OnNext(7);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([1, 3, 5, 7]);


        publisher.OnNext(8);
        list.AssertEqual([1, 3, 5, 7, 8]);

        sampler.OnNext(Unit.Default);
        list.AssertEqual([1, 3, 5, 7, 8]);
        sampler.OnNext(Unit.Default);
        sampler.OnNext(Unit.Default);
        list.AssertEqual([1, 3, 5, 7, 8]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }


    // ThrottleLatestFrame

    [Fact]
    public void ThrottleLatestFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.ThrottleLatestFrame(3, frameProvider).ToLiveList();

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
        list.AssertEqual([1, 10000]);
        publisher.OnNext(2);
        list.AssertEqual([1, 10000, 2]);
        publisher.OnNext(20);
        publisher.OnNext(200);

        frameProvider.Advance(3);
        list.AssertEqual([1, 10000, 2, 200]);
        publisher.OnNext(3);

        publisher.OnCompleted();

        list.AssertEqual([1, 10000, 2, 200, 3]);
        list.AssertIsCompleted();
    }
}
