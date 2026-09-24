namespace R3.Tests;

public class LiveListTest
{
    [Fact]
    public void FromEvent()
    {
        var publisher = new Subject<int>();
        var list = publisher.ToLiveList();

        list.AssertEqual([]);

        publisher.OnNext(10);
        list.AssertEqual([10]);

        publisher.OnNext(20);
        list.AssertEqual([10, 20]);

        publisher.OnNext(30);
        list.AssertEqual([10, 20, 30]);

        list.Dispose();

        publisher.OnNext(40);
        list.AssertEqual([10, 20, 30]);
    }

    [Fact]
    public void BufferSize()
    {
        var publisher = new Subject<int>();
        var list = publisher.ToLiveList(bufferSize: 5);

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);
        publisher.OnNext(40);
        publisher.OnNext(50);

        list.AssertEqual([10, 20, 30, 40, 50]);

        publisher.OnNext(60);

        list.AssertEqual([20, 30, 40, 50, 60]);

        list[0].ShouldBe(20);
        list[1].ShouldBe(30);
        list[2].ShouldBe(40);
        list[3].ShouldBe(50);
        list[4].ShouldBe(60);
    }

    /// <summary>
    /// -1 used to double as the internal "unlimited" sentinel, so this guarantees that a
    /// negative bufferSize is rejected up front instead of being silently accepted as a
    /// request for a list with no size limit.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(int.MinValue)]
    public void NegativeBufferSizeIsRejected(int bufferSize)
    {
        var publisher = new Subject<int>();

        var ex = Should.Throw<ArgumentOutOfRangeException>(() => publisher.ToLiveList(bufferSize));
        ex.ParamName.ShouldBe("bufferSize");
    }

    /// <summary>
    /// Guarantees that an out-of-range index on a LiveList with no size limit throws
    /// ArgumentOutOfRangeException, instead of letting RingBuffer's non-validating indexer
    /// silently return default or a stale value.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(3)]
    [InlineData(4)]
    public void UnboundedIndexerThrowsForOutOfRangeIndex(int index)
    {
        var publisher = new Subject<int>();
        using var list = publisher.ToLiveList();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);

        var ex = Should.Throw<ArgumentOutOfRangeException>(() => _ = list[index]);
        ex.ParamName.ShouldBe("index");
    }

    /// <summary>
    /// Guarantees that an out-of-range index still throws after the bounded ring buffer has
    /// wrapped around. Without the index check, RingBuffer's indexer masks the index with
    /// the buffer capacity and quietly returns an already discarded element.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(5)]
    [InlineData(6)]
    public void BoundedIndexerThrowsForOutOfRangeIndex(int index)
    {
        var publisher = new Subject<int>();
        using var list = publisher.ToLiveList(bufferSize: 5);

        foreach (var i in Enumerable.Range(1, 12))
        {
            publisher.OnNext(i);
        }

        list.Count.ShouldBe(5);

        var ex = Should.Throw<ArgumentOutOfRangeException>(() => _ = list[index]);
        ex.ParamName.ShouldBe("index");
    }

    /// <summary>
    /// With bufferSize 5 the internal capacity is 8, so 12 values leave the ring wrapped and
    /// GetSpan returns two segments instead of one. This guarantees that the indexer,
    /// ToArray, enumeration and both ForEach overloads all observe the retained values in
    /// arrival order across that split.
    /// </summary>
    [Fact]
    public void BoundedBufferKeepsOrderAfterWrapAround()
    {
        var publisher = new Subject<int>();
        using var list = publisher.ToLiveList(bufferSize: 5);

        foreach (var i in Enumerable.Range(1, 12))
        {
            publisher.OnNext(i);
        }

        int[] expected = [8, 9, 10, 11, 12];

        list.Count.ShouldBe(expected.Length);
        list.ToArray().ShouldBe(expected);
        list.AssertEqual(expected); // enumerate through IEnumerable<T>

        for (var i = 0; i < expected.Length; i++)
        {
            list[i].ShouldBe(expected[i]);
        }

        var collected = new List<int>();
        list.ForEach(collected.Add);
        collected.ShouldBe(expected);

        var collectedWithState = new List<int>();
        list.ForEach(collectedWithState, static (x, state) => state.Add(x));
        collectedWithState.ShouldBe(expected);
    }

    /// <summary>
    /// The unbounded LiveList stores values in RingBuffer, whose initial capacity is 8.
    /// This guarantees that growing past that capacity keeps every value in arrival order,
    /// which the previous List based storage got for free.
    /// </summary>
    [Fact]
    public void UnboundedBufferGrowsBeyondInitialCapacity()
    {
        var publisher = new Subject<int>();
        using var list = publisher.ToLiveList();

        var expected = Enumerable.Range(0, 100).ToArray();

        foreach (var i in expected)
        {
            publisher.OnNext(i);
        }

        list.Count.ShouldBe(expected.Length);
        list.ToArray().ShouldBe(expected);
        list[0].ShouldBe(0);
        list[99].ShouldBe(99);
    }
}
