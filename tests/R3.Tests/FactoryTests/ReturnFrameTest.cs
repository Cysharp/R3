using System.Runtime.InteropServices;

namespace R3.Tests.FactoryTests;

public class ReturnFrameTest
{
    [Fact]
    public void UnitTest()
    {
        var frameProvider = new FakeFrameProvider();
        var cts = new CancellationTokenSource();

        var list = Observable.YieldFrame(frameProvider, cts.Token).ToLiveList();
        list.AssertIsNotCompleted();

        frameProvider.Advance();
        list.AssertIsCompleted();
        list.AssertEqual([Unit.Default]);
    }

    [Fact]
    public void ValueTest()
    {
        {
            var frameProvider = new FakeFrameProvider();
            var cts = new CancellationTokenSource();

            var list = Observable.ReturnFrame(10, frameProvider, cts.Token).ToLiveList();
            list.AssertIsNotCompleted();

            frameProvider.Advance();
            list.AssertIsCompleted();
            list.AssertEqual([10]);
        }
        {
            var frameProvider = new FakeFrameProvider();
            var cts = new CancellationTokenSource();

            var list = Observable.ReturnFrame(10, frameProvider, cts.Token).ToLiveList();
            list.AssertIsNotCompleted();

            cts.Cancel();
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void TimeTest()
    {
        var frameProvider = new FakeFrameProvider();
        var cts = new CancellationTokenSource();

        var list = Observable.ReturnFrame(10, 5, frameProvider, cts.Token).ToLiveList();
        list.AssertIsNotCompleted();

        frameProvider.Advance(4);
        list.AssertIsNotCompleted();

        frameProvider.Advance(1);
        list.AssertIsCompleted();
        list.AssertEqual([10]);
    }

    [Fact]
    public void NextFrameTest()
    {
        {
            var frameProvider = new FakeFrameProvider();
            var cts = new CancellationTokenSource();

            var list = Observable.NextFrame(frameProvider, cts.Token).ToLiveList();
            list.AssertIsNotCompleted();

            frameProvider.Advance(1); // same frame, not run.
            list.AssertIsNotCompleted();

            frameProvider.Advance(1); // diffrent frame, ok to run.
            list.AssertIsCompleted();
            list.AssertEqual(Unit.Default);
        }
        {
            var frameProvider = new FakeFrameProvider(); // use custom fake
            var cts = new CancellationTokenSource();

            var list = Observable.YieldFrame(frameProvider, cts.Token).ToLiveList();
            list.AssertIsNotCompleted();

            // ReturnFrame run same frame.
            frameProvider.Advance(1);

            list.AssertIsCompleted();
            list.AssertEqual(Unit.Default);
        }
    }
}
