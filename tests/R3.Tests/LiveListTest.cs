namespace R3.Tests;

public class LiveListTest
{
    [Fact]
    public void FromEvent()
    {
        var publisher = new Publisher<int, Unit>();
        var list = publisher.ToLiveList();

        list.AssertEqual([]);

        publisher.PublishOnNext(10);
        list.AssertEqual([10]);

        publisher.PublishOnNext(20);
        list.AssertEqual([10, 20]);

        publisher.PublishOnNext(30);
        list.AssertEqual([10, 20, 30]);

        list.Dispose();

        publisher.PublishOnNext(40);
        list.AssertEqual([10, 20, 30]);
    }

    [Fact]
    public void BufferSize()
    {
        var publisher = new Publisher<int, Unit>();
        var list = publisher.ToLiveList(bufferSize: 5);

        publisher.PublishOnNext(10);
        publisher.PublishOnNext(20);
        publisher.PublishOnNext(30);
        publisher.PublishOnNext(40);
        publisher.PublishOnNext(50);

        list.AssertEqual([10, 20, 30, 40, 50]);

        publisher.PublishOnNext(60);

        list.AssertEqual([20, 30, 40, 50, 60]);

        list[0].Should().Be(20);
        list[1].Should().Be(30);
        list[2].Should().Be(40);
        list[3].Should().Be(50);
        list[4].Should().Be(60);
    }
}
