namespace R3.Tests.FactoryTests;

public class TimerFrameTest
{

    [Fact]
    public void TimerSingle()
    {
        {
            var fakeTime = new FakeFrameProvider();
            var list = Observable.TimerFrame(0, fakeTime).ToLiveList();
            fakeTime.Advance(1);
            list.AssertIsCompleted();
            list.AssertEqual([Unit.Default]);
        }
        {
            var fakeTime = new FakeFrameProvider();
            var list = Observable.TimerFrame(1, fakeTime).ToLiveList();
            fakeTime.Advance(1);
            list.AssertIsCompleted();
            list.AssertEqual([Unit.Default]);
        }
        {
            var fakeTime = new FakeFrameProvider();
            var list = Observable.TimerFrame(2, fakeTime).ToLiveList();
            fakeTime.Advance(2);
            list.AssertIsCompleted();
            list.AssertEqual([Unit.Default]);
        }
    }

    [Fact]
    public void TimerSingle2()
    {
        var fakeTime = new FakeFrameProvider();

        var list = Observable.TimerFrame(5, fakeTime).ToLiveList();

        fakeTime.Advance(4);
        list.AssertIsNotCompleted();

        fakeTime.Advance(1);
        list.AssertIsCompleted();
        list.AssertEqual(new[] { Unit.Default });
    }

    [Fact]
    public void TimerMulti()
    {
        var cts = new CancellationTokenSource();
        var fakeTime = new FakeFrameProvider();

        var list = Observable.TimerFrame(5, 8, fakeTime, cts.Token).ToLiveList();

        fakeTime.Advance(4);
        list.AssertIsNotCompleted();

        fakeTime.Advance(1);
        list.AssertIsNotCompleted();
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(7);
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(1);
        list.AssertEqual([Unit.Default, Unit.Default]);

        fakeTime.Advance(8);
        list.AssertEqual([Unit.Default, Unit.Default, Unit.Default]);

        cts.Cancel();
        list.AssertIsCompleted();
    }

    [Fact]
    public void Interval()
    {
        var cts = new CancellationTokenSource();
        var fakeTime = new FakeFrameProvider();

        var list = Observable.IntervalFrame(5, fakeTime, cts.Token).ToLiveList();

        fakeTime.Advance(4);
        list.AssertIsNotCompleted();

        fakeTime.Advance(1);
        list.AssertIsNotCompleted();
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(4);
        list.AssertEqual([Unit.Default]);

        fakeTime.Advance(1);
        list.AssertEqual([Unit.Default, Unit.Default]);

        fakeTime.Advance(5);
        list.AssertEqual([Unit.Default, Unit.Default, Unit.Default]);

        cts.Cancel();
        list.AssertIsCompleted();
    }
}
