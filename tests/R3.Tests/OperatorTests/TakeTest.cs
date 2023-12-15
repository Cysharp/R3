using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class TakeTest
{
    [Fact]
    public async Task Take()
    {
        var xs = await Observable.Range(1, 10).Take(3).ToArrayAsync();

        xs.Should().Equal([1, 2, 3]);

        var timeProvider = new FakeTimeProvider();

        var publisher = new Publisher<int>();
        var list = publisher.Take(TimeSpan.FromSeconds(5), timeProvider).ToLiveList();

        publisher.PublishOnNext(1);
        publisher.PublishOnNext(10);
        publisher.PublishOnNext(100);
        list.AssertEqual([1, 10, 100]);

        timeProvider.Advance(TimeSpan.FromSeconds(3));

        publisher.PublishOnNext(1000);
        publisher.PublishOnNext(10000);
        list.AssertEqual([1, 10, 100, 1000, 10000]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        list.AssertIsCompleted();
    }

    [Fact]
    public void TakeFrame()
    {
        var frameProvider = new ManualFrameProvider();

        var publisher = new Publisher<int>();
        var list = publisher.TakeFrame(5, frameProvider).ToLiveList();
        publisher.PublishOnNext(1);
        publisher.PublishOnNext(10);
        publisher.PublishOnNext(100);
        list.AssertEqual([1, 10, 100]);

        frameProvider.Advance(3);
        publisher.PublishOnNext(1000);
        publisher.PublishOnNext(10000);
        list.AssertEqual([1, 10, 100, 1000, 10000]);
        frameProvider.Advance(2);
        list.AssertIsNotCompleted();
        frameProvider.Advance(1);
        list.AssertIsCompleted();
    }

    [Fact]
    public void TakeFrame2()
    {
        var frameProvider = new ManualFrameProvider();

        var list = Observable.EveryUpdate(frameProvider)
            .Select(x => (int)frameProvider.GetFrameCount())
            .TakeFrame(5, frameProvider)
            .ToLiveList();

        frameProvider.Advance(3);
        list.AssertEqual([0, 1, 2]);

        frameProvider.Advance(2);
        list.AssertEqual([0, 1, 2, 3, 4]);
        frameProvider.Advance(1);

        list.AssertIsCompleted();
    }
}
