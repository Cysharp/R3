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

        var publisher = new Subject<int>();
        var list = publisher.Take(TimeSpan.FromSeconds(5), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        timeProvider.Advance(TimeSpan.FromSeconds(3));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1, 10, 100, 1000, 10000]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        list.AssertIsCompleted();
    }

    [Fact]
    public void TakeFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.TakeFrame(5, frameProvider).ToLiveList();
        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        frameProvider.Advance(3);
        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([1, 10, 100, 1000, 10000]);
        frameProvider.Advance(2);
        list.AssertIsCompleted();
    }

    [Fact]
    public void TakeFrame2()
    {
        var frameProvider = new FakeFrameProvider();

        var list = Observable.EveryUpdate(frameProvider)
            .Select(x => frameProvider.GetFrameCount())
            .TakeFrame(5, frameProvider)
            .ToLiveList();

        frameProvider.Advance(3);
        list.AssertEqual([0, 1, 2]);

        frameProvider.Advance(2);

        // not guranteed everyupdate and takeframe which call first
        // list.AssertEqual([0, 1, 2, 3]);
        list.AssertIsCompleted();
    }
}
