using System;
using System.Reactive.Linq;
using Xunit.Sdk;

namespace R3.Tests.OperatorTests;

public class SkipTest
{
    [Fact]
    public async Task Skip()
    {
        var xs = await Observable.Range(1, 10).Skip(3).ToArrayAsync();

        xs.Should().Equal([4, 5, 6, 7, 8, 9, 10]);

        // skip zero
        var ys = await Observable.Range(1, 10).Skip(0).ToArrayAsync();
        ys.Should().Equal([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void SkipTime()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.Skip(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));

        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        list.AssertEqual([2, 20, 200]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void SkipFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.SkipFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        frameProvider.Advance(1);

        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        list.AssertEqual([2, 20, 200]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }
}
