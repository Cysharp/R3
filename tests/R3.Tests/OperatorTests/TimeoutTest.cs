namespace R3.Tests.OperatorTests;

public class TimeoutTest
{
    [Fact]
    public void Timeout()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.Timeout(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1, 1000, 10000]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([1, 1000, 10000]);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([1, 1000, 10000]);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertIsCompleted();
        list.Result.Exception!.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public void TimeoutFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.TimeoutFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1, 1000, 10000]);

        frameProvider.Advance(1);
        list.AssertEqual([1, 1000, 10000]);
        frameProvider.Advance(1);
        list.AssertEqual([1, 1000, 10000]);
        frameProvider.Advance(1);
        list.AssertIsCompleted();
        list.Result.Exception!.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public void Timeout2()
    {
        var fakeTimeProvider = new FakeTimeProvider();
        using var subject = new R3.Subject<int>();

        var results =
            subject.Timeout(TimeSpan.FromSeconds(3), fakeTimeProvider)
                .Materialize()
                .ToLiveList();

        // Timeout
        fakeTimeProvider.Advance(5);

        results.ShouldBeEmpty();
    }

    [Fact]
    public void TimeoutFrame2()
    {
        var fakeFrameProvider = new FakeFrameProvider();
        using var subject = new R3.Subject<int>();

        var results =
            subject.TimeoutFrame(3, fakeFrameProvider)
                .Materialize()
                .ToLiveList();

        // Timeout
        fakeFrameProvider.Advance(5);

        results.ShouldBeEmpty();
    }
}
