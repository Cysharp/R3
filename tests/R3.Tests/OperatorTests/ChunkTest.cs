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

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], []);

        publisher.OnNext(4);

        publisher.OnCompleted();

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], [], [4]);

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
        var frameProvider = new ManualFrameProvider();

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
        var frameProvider = new ManualFrameProvider();

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

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], []);

        publisher.OnNext(4);

        publisher.OnCompleted();

        list.AssertEqual([1, 10], [100], [1000, 10000], [50, 2], [3], [], [4]);

        list.AssertIsCompleted();
    }
}
