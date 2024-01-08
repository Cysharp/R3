namespace R3.Tests.OperatorTests;

public class ObserveOnTest
{
    // null synccontext(TimeProvider.System)
    [Fact]
    public async Task ThreadPool()
    {
        var l = new List<int>();
        await Observable.Range(1, 10).ObserveOn((SynchronizationContext?)null).ForEachAsync(x =>
        {
            Thread.CurrentThread.IsThreadPoolThread.Should().BeTrue();
            lock (l)
            {
                l.Add(x);
            }
        });

        l.Order().Should().Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void SyncContext()
    {
        var syncContext = new CustomSyncContext();
        var list = Observable.Range(1, 10).ObserveOn(syncContext).ToLiveList();

        syncContext.PostCount.Should().Be(11); // OnNext + OnCompleted
        list.AssertEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void TimeProvider()
    {
        var fakeTime = new FakeTimeProvider();
        var publisher = new Subject<int>();

        using var list = publisher.ObserveOn(fakeTime).ToLiveList();


        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);

        list.AssertEqual([10, 20, 30]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void FrameProvider()
    {
        var fakeTime = new FakeFrameProvider();
        var publisher = new Subject<int>();

        using var list = publisher.ObserveOn(fakeTime).ToLiveList();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);

        list.AssertEqual([]);

        fakeTime.Advance();
        list.AssertEqual([10, 20, 30]);

        publisher.OnCompleted();
        list.AssertIsNotCompleted();

        fakeTime.Advance();
        list.AssertIsCompleted();
    }

    [Fact]
    public void FrameProvider2()
    {

        var fakeTime = new FakeFrameProvider();
        var publisher = new Subject<int>();

        using var list = publisher.ObserveOn(fakeTime)
            .Do(x =>
            {
                if (x == 20)
                {
                    publisher.OnNext(99);
                }
            })
            .ToLiveList();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);

        list.AssertEqual([]);

        fakeTime.Advance();
        list.AssertEqual([10, 20, 30]);

        fakeTime.Advance();
        list.AssertEqual([10, 20, 30, 99]);

        publisher.OnNext(40);
        fakeTime.Advance();
        list.AssertEqual([10, 20, 30, 99, 40]);

        publisher.OnCompleted();
        list.AssertIsNotCompleted();

        fakeTime.Advance();
        list.AssertIsCompleted();
    }
}


file class CustomSyncContext : SynchronizationContext
{
    public int PostCount;

    public override void Post(SendOrPostCallback d, object? state)
    {
        PostCount++;
        d(state);
    }
}
