using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class SkipLastTest
{
    [Fact]
    public async Task Skip()
    {
        var xs = await Observable.Range(1, 10).SkipLast(3).ToArrayAsync();
        xs.Should().Equal([1, 2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void SkipTime()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.SkipLast(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));
        publisher.OnNext(100);
        publisher.OnNext(1000);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        publisher.OnNext(2);
        publisher.OnNext(20);

        publisher.OnCompleted();

        list.AssertEqual([1, 10]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void SkipFrame2()
    {
        var frameProvider = new FakeFrameProvider();
        var cts = new CancellationTokenSource();

        var list = Observable.EveryUpdate(frameProvider, cts.Token)
            .Select(x => frameProvider.GetFrameCount())
            .SkipLastFrame(3, frameProvider)
            .ToLiveList();

        frameProvider.Advance(3); // 0, 1, 2
        list.AssertEqual([]);
        frameProvider.Advance(1); // 3
        list.AssertEqual([0]);

        frameProvider.Advance(1); // 4
        list.AssertEqual([0, 1]);

        frameProvider.Advance(1); // 5
        list.AssertEqual([0, 1, 2]);

        frameProvider.Advance(1); // 6
        list.AssertEqual([0, 1, 2, 3]);

        cts.Cancel(); // stop and OnCompleted(frame no is adavnced +1)

        list.AssertEqual([0, 1, 2, 3, 4]);
        list.AssertIsCompleted();
    }
}
