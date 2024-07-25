namespace R3.Tests.OperatorTests;

public class ChunkTest
{
    [Fact]
    public void Chunk()
    {
        var subject = new Subject<int>();

        var list = subject.Chunk(3).ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        list.AssertEqual([[1, 2, 3]]);

        subject.OnNext(4);
        subject.OnNext(5);
        subject.OnNext(6);
        list.AssertEqual([1, 2, 3], [4, 5, 6]);

        subject.OnNext(7);
        subject.OnNext(8);
        subject.OnNext(9);
        list.AssertEqual([1, 2, 3], [4, 5, 6], [7, 8, 9]);

        subject.OnNext(10);

        subject.OnCompleted();

        list.AssertEqual([1, 2, 3], [4, 5, 6], [7, 8, 9], [10]);

        list.AssertIsCompleted();
    }

    // ChunkTime
    [Fact]
    public void ChunkTime()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.Chunk(TimeSpan.FromSeconds(3), timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(2));

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));


        list.AssertEqual([[1, 10, 100, 1000, 10000]]);

        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        timeProvider.Advance(TimeSpan.FromSeconds(3));
        list.AssertEqual([[1, 10, 100, 1000, 10000], [2, 20, 200]]);

        publisher.OnNext(2300);


        publisher.OnCompleted();

        list.AssertEqual([[1, 10, 100, 1000, 10000], [2, 20, 200], [2300]]);

        list.AssertIsCompleted();
    }

    // ChunkTimeAndCount
    [Fact]
    public void ChunkTimeAndCount()
    {
        var timeProvider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var list = publisher.Chunk(TimeSpan.FromSeconds(3), 2, timeProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        list.AssertEqual([1, 10]);
        publisher.OnNext(100);
        list.AssertEqual([1, 10]);

        timeProvider.Advance(TimeSpan.FromSeconds(3));

        list.AssertEqual([1, 10], [100]);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        publisher.OnNext(50);
        list.AssertEqual([1, 10], [100], [1000, 10000]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([1, 10], [100], [1000, 10000]);

        publisher.OnNext(2);
        publisher.OnNext(3);
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2]);
        timeProvider.Advance(TimeSpan.FromSeconds(2));
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2]);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3]);

        timeProvider.Advance(TimeSpan.FromSeconds(3));

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3]);

        publisher.OnNext(4);

        publisher.OnCompleted();

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], [4]);

        list.AssertIsCompleted();
    }

    // ChunkWindowBoundary
    [Fact]
    public void ChunkWindowBoundary()
    {
        var publisher = new Subject<int>();
        var boundary = new Subject<Unit>();
        var list = publisher.Chunk(boundary).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        boundary.OnNext(Unit.Default);
        list.AssertEqual([[1, 10, 100]]);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([[1, 10, 100]]);

        boundary.OnNext(Unit.Default);
        list.AssertEqual([[1, 10, 100], [1000, 10000]]);

        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        list.AssertEqual([[1, 10, 100], [1000, 10000]]);

        boundary.OnNext(Unit.Default);
        list.AssertEqual([[1, 10, 100], [1000, 10000], [2, 20, 200]]);

        publisher.OnNext(500);

        publisher.OnCompleted();

        list.AssertEqual([[1, 10, 100], [1000, 10000], [2, 20, 200], [500]]);
        list.AssertIsCompleted();
    }

    // ChunkFrame
    [Fact]
    public void ChunkFrame()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.ChunkFrame(3, frameProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        frameProvider.Advance(2);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([]);

        frameProvider.Advance(1);

        list.AssertEqual([[1, 10, 100, 1000, 10000]]);

        publisher.OnNext(2);
        publisher.OnNext(20);
        frameProvider.Advance(3);
        list.AssertEqual([[1, 10, 100, 1000, 10000], [2, 20]]);

        publisher.OnNext(50);


        publisher.OnCompleted();

        list.AssertEqual([[1, 10, 100, 1000, 10000], [2, 20], [50]]);


        list.AssertIsCompleted();
    }

    // ChunkFrameAndCount
    [Fact]
    public void ChunkFrameAndCount()
    {
        var frameProvider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var list = publisher.ChunkFrame(3, 2, frameProvider).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        list.AssertEqual([1, 10]);
        publisher.OnNext(100);
        list.AssertEqual([1, 10]);

        frameProvider.Advance(3);

        list.AssertEqual([1, 10], [100]);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        publisher.OnNext(50);
        list.AssertEqual([1, 10], [100], [1000, 10000]);

        frameProvider.Advance(1);
        list.AssertEqual([1, 10], [100], [1000, 10000]);

        publisher.OnNext(2);
        publisher.OnNext(3);
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2]);
        frameProvider.Advance(2);
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2]);

        frameProvider.Advance(1);
        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3]);

        frameProvider.Advance(3);

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3]);

        publisher.OnNext(4);

        publisher.OnCompleted();

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], [4]);

        list.AssertIsCompleted();
    }

    // Async
    [Fact]
    public void ChunkAsync()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher = new Subject<int>();
        var tp = new FakeTimeProvider();
        var list = publisher.Chunk(async (x, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3), tp);

        }).ToLiveList();

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        list.AssertEqual([]);

        tp.Advance(3);

        list.AssertEqual([[1, 10, 100]]);

        publisher.OnNext(1000);
        publisher.OnNext(10000);
        list.AssertEqual([[1, 10, 100]]);

        tp.Advance(3);
        list.AssertEqual([[1, 10, 100], [1000, 10000]]);

        publisher.OnNext(2);
        publisher.OnNext(20);
        publisher.OnNext(200);

        list.AssertEqual([[1, 10, 100], [1000, 10000]]);

        tp.Advance(1);
        list.AssertEqual([[1, 10, 100], [1000, 10000]]);

        tp.Advance(2);
        list.AssertEqual([[1, 10, 100], [1000, 10000], [2, 20, 200]]);

        publisher.OnNext(500);

        publisher.OnCompleted();

        list.AssertEqual([[1, 10, 100], [1000, 10000], [2, 20, 200], [500]]);
        list.AssertIsCompleted();
    }

    // count + skip
    [Fact]
    public async Task ChunkCountSkip()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        {
            var xs = await Observable.Range(1, 10).Chunk(3, 1).ToArrayAsync();

            xs[0].Should().Equal(1, 2, 3);
            xs[1].Should().Equal(2, 3, 4);
            xs[2].Should().Equal(3, 4, 5);
            xs[3].Should().Equal(4, 5, 6);
            xs[4].Should().Equal(5, 6, 7);
            xs[5].Should().Equal(6, 7, 8);
            xs[6].Should().Equal(7, 8, 9);
            xs[7].Should().Equal(8, 9, 10);
            xs[8].Should().Equal(9, 10);
            xs[9].Should().Equal(10);
        }

        // count == skip
        {
            var xs = await Observable.Range(1, 10).Chunk(3, 3).ToArrayAsync();

            xs[0].Should().Equal(1, 2, 3);
            xs[1].Should().Equal(4, 5, 6);
            xs[2].Should().Equal(7, 8, 9);
            xs[3].Should().Equal(10);
        }

        // count < skip
        {
            var xs = await Observable.Range(1, 20).Chunk(3, 5).ToArrayAsync();

            xs[0].Should().Equal(1, 2, 3);
            xs[1].Should().Equal(6, 7, 8);
            xs[2].Should().Equal(11, 12, 13);
            xs[3].Should().Equal(16, 17, 18);
        }
    }

    // FrameOperator should start on OnNext

    [Fact]
    public void ChunkFrame2()
    {
        var publisher = new Subject<int>();
        var provider = new FakeFrameProvider();

        using var list = publisher.ChunkFrame(5, provider).ToLiveList();

        provider.Advance(8); // tick 8 frame before OnNext(time is not starting)

        publisher.OnNext(1); // start timer
        publisher.OnNext(2);
        publisher.OnNext(3);

        provider.Advance(2);

        list.AssertEmpty();

        provider.Advance(3);

        list[0].Should().Equal(1, 2, 3);

        provider.Advance(4); // timer is stopping

        publisher.OnNext(4);

        provider.Advance(1);

        list.Count.Should().Be(1);

        publisher.OnNext(5);

        provider.Advance(4);

        list[1].Should().Equal(4, 5);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ChunkFrameCount2()
    {
        var publisher = new Subject<int>();
        var provider = new FakeFrameProvider();

        using var list = publisher.ChunkFrame(frameCount: 5, count: 10, provider).ToLiveList();

        provider.Advance(8); // tick 8 frame before OnNext(time is not starting)

        publisher.OnNext(1); // start timer
        publisher.OnNext(2);
        publisher.OnNext(3);

        provider.Advance(2);

        list.AssertEmpty();

        provider.Advance(3);

        list[0].Should().Equal(1, 2, 3);

        provider.Advance(4); // timer is stopping

        publisher.OnNext(4);

        provider.Advance(1);

        list.Count.Should().Be(1);

        publisher.OnNext(5);

        provider.Advance(4);

        list[1].Should().Equal(4, 5);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ChunkFrameCount2_countfull()
    {
        var publisher = new Subject<int>();
        var provider = new FakeFrameProvider();

        using var list = publisher.ChunkFrame(frameCount: 5, count: 2, provider).ToLiveList();

        provider.Advance(8); // tick 8 frame before OnNext(time is not starting)

        publisher.OnNext(1); // start timer

        provider.Advance(3);

        publisher.OnNext(2); // count full, timer reset

        list[0].Should().Equal(1, 2);

        publisher.OnNext(3);

        provider.Advance(2);

        list.Count().Should().Be(1);

        provider.Advance(3);

        list[1].Should().Equal(3);
    }
}


