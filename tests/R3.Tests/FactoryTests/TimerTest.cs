namespace R3.Tests.FactoryTests;

public class TimerTest
{
    [Fact]
    public void TimerSingle()
    {
        {
            var fakeTime = new FakeTimeProvider();

            var list = Observable.Timer(TimeSpan.FromSeconds(5), fakeTime).ToLiveList();

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertIsCompleted();
            list.AssertEqual([Unit.Default]);
        }
        {
            var fakeTime = new FakeTimeProvider();

            var dueTime = fakeTime.GetUtcNow().AddSeconds(5);

            var list = Observable.Timer(dueTime, fakeTime).ToLiveList();

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertIsCompleted();
            list.AssertEqual([Unit.Default]);
        }
    }

    [Fact]
    public void TimerMulti()
    {
        var cts = new CancellationTokenSource();
        var fakeTime = new FakeTimeProvider();

        var list = Observable.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8), fakeTime, cts.Token).ToLiveList();

        fakeTime.Advance(TimeSpan.FromSeconds(4));
        list.AssertIsNotCompleted();

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        list.AssertIsNotCompleted();
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(7));
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([Unit.Default, Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(8));
        list.AssertEqual([Unit.Default, Unit.Default, Unit.Default]);

        cts.Cancel();
        list.AssertIsCompleted();
    }

    [Fact]
    public void Interval()
    {
        var cts = new CancellationTokenSource();
        var fakeTime = new FakeTimeProvider();

        var list = Observable.Interval(TimeSpan.FromSeconds(5), fakeTime, cts.Token).ToLiveList();

        fakeTime.Advance(TimeSpan.FromSeconds(4));
        list.AssertIsNotCompleted();

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        list.AssertIsNotCompleted();
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(4));
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([Unit.Default, Unit.Default]);

        fakeTime.Advance(TimeSpan.FromSeconds(5));
        list.AssertEqual([Unit.Default, Unit.Default, Unit.Default]);

        cts.Cancel();
        list.AssertIsCompleted();
    }

}
