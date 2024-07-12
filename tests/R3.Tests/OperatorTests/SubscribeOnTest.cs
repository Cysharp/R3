using Newtonsoft.Json.Linq;

namespace R3.Tests.OperatorTests;

public class SubscribeOnTest
{

    // null synccontext(TimeProvider.System)
    [Fact]
    public async Task ThreadPool()
    {
        var values = await Observable.Range(1, 10)
            .Do(onSubscribe: () => Thread.CurrentThread.IsThreadPoolThread.Should().BeTrue())
            .SubscribeOnThreadPool()
            .ToArrayAsync();
        values.Should().Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task Synchronize()
    {
        var gate = new object();
        var values = await Observable.Range(1, 10)
            .SubscribeOnSynchronize(gate)
            .ToArrayAsync();
        values.Should().Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public async Task SyncContext()
    {
        var syncContext = new CustomSyncContext();
        syncContext.IsInSyncContext.Should().BeFalse();
        var values = await Observable.Range(1, 10)
            .Do(onSubscribe: () => syncContext.IsInSyncContext.Should().BeTrue())
            .SubscribeOn(syncContext)
            .ToArrayAsync();
        values.Should().Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        syncContext.PostCount.Should().Be(1);
    }

    [Fact]
    public void TimeProvider()
    {
        var fakeTime = new ImmediateFakeTiemr();
        var subscribed = false;
        using var list = Observable.Range(1, 10)
            .Do(onSubscribe: () => subscribed = true)
            .SubscribeOn(fakeTime)
            .ToLiveList();


        subscribed.Should().BeFalse();

        fakeTime.Advance();
        subscribed.Should().BeTrue();

        list.AssertEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }


    [Fact]
    public void FrameProvider()
    {

        var fakeTime = new FakeFrameProvider();
        var subscribed = false;
        using var list = Observable.Range(1, 10)
            .Do(onSubscribe: () => subscribed = true)
            .SubscribeOn(fakeTime)
            .ToLiveList();


        subscribed.Should().BeFalse();

        fakeTime.Advance();
        subscribed.Should().BeTrue();

        list.AssertEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }
}

file class CustomSyncContext : SynchronizationContext
{
    public int PostCount;
    public bool IsInSyncContext { get; set; }

    public override void Post(SendOrPostCallback d, object? state)
    {
        IsInSyncContext = true;
        PostCount++;
        d(state);
        IsInSyncContext = false;
    }
}

file class ImmediateFakeTiemr : TimeProvider
{
    List<Timer> timers = new();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var t = new Timer(callback, state);
        timers.Add(t);
        return t;
    }

    public void Advance()
    {
        foreach (var item in timers)
        {
            item.Wakeup();
        }
    }

    class Timer(TimerCallback callback, object? state) : ITimer
    {
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            return false;
        }

        public bool Wakeup()
        {
            callback(state);
            return true;
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
