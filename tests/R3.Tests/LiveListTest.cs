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

        list[0].Should().Be(20);
        list[1].Should().Be(30);
        list[2].Should().Be(40);
        list[3].Should().Be(50);
        list[4].Should().Be(60);
    }
}
