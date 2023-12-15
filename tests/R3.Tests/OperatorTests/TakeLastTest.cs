using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class TakeLastTest
{
    [Fact]
    public async Task Take()
    {
        var xs = await Observable.Range(1, 10).TakeLast(3).ToArrayAsync();
        xs.Should().Equal([8, 9, 10]);
    }

    [Fact]
    public void TakeTime()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.TakeLast(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        publisher.OnNext(100);
        publisher.OnNext(1000);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        publisher.OnNext(2);
        publisher.OnNext(20);

        publisher.OnCompleted();

        list.AssertEqual([100, 1000, 2, 20]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void TakeFrame2()
    {
        var frameProvider = new ManualFrameProvider();
        var cts = new CancellationTokenSource();

        var list = Observable.EveryUpdate(frameProvider, cts.Token, cancelImmediately: true)
            .Select(x => frameProvider.GetFrameCount())
            .TakeLastFrame(3, frameProvider)
            .ToLiveList();

        frameProvider.Advance(3); // 0, 1, 2
        list.AssertEqual([]);

        frameProvider.Advance(2); // 3, 4
        frameProvider.Advance(1); // 5

        cts.Cancel(); // stop and OnCompleted

        list.AssertEqual([3, 4, 5]);


        list.AssertIsCompleted();
    }
}
