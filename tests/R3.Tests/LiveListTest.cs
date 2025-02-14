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
}
