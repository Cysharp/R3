namespace R3.Tests.OperatorTests;

public class DebounceThrottleFirstSampleTest
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
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([1]);
        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        timeProvider.Advance(TimeSpan.FromSeconds(3));
        list.AssertEqual([1, 2]);
        publisher.OnNext(3);

        publisher.OnCompleted();

        list.AssertEqual([1, 2]);
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
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        frameProvider.Advance(1);
        list.AssertEqual([1]);
        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        frameProvider.Advance(3);
        list.AssertEqual([1, 2]);
        publisher.OnNext(3);

        publisher.OnCompleted();

        list.AssertEqual([1, 2]);
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
}
