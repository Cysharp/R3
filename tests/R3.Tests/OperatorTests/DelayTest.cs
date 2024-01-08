namespace R3.Tests.OperatorTests;

public class DelayTest
{
    [Fact]
    public void Delay()
    {
        var provider = new FakeTimeProvider();
        var subject = new Subject<int>();

        var e = new List<Exception>();

        var list = subject.Delay(TimeSpan.FromSeconds(3), provider).Do(onErrorResume: ex => e.Add(ex)).ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnErrorResume(new Exception("a"));
        subject.OnNext(4);
        subject.OnNext(5);

        list.AssertEqual([]);
        provider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([]);
        provider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([]);
        e.Should().BeEmpty();
        provider.Advance(TimeSpan.FromSeconds(1));

        list.AssertEqual([1, 2, 3, 4, 5]);
        e[0].Message.Should().Be("a");

        list.Clear();

        subject.OnNext(10);
        provider.Advance(TimeSpan.FromSeconds(1));
        subject.OnNext(20);
        provider.Advance(TimeSpan.FromSeconds(1));
        subject.OnNext(30);
        provider.Advance(TimeSpan.FromSeconds(1));

        list.AssertEqual([10]);
        provider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([10, 20]);
        provider.Advance(TimeSpan.FromSeconds(1));
        list.AssertEqual([10, 20, 30]);

        subject.OnCompleted();
        list.AssertIsNotCompleted();
        provider.Advance(TimeSpan.FromSeconds(3));
        list.AssertIsCompleted();
    }

    [Fact]
    public void DelayFrame()
    {
        var provider = new FakeFrameProvider();
        var subject = new Subject<int>();

        var e = new List<Exception>();

        var list = subject.DelayFrame((3), provider).Do(onErrorResume: ex => e.Add(ex)).ToLiveList();
        provider.Advance(10);

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnErrorResume(new Exception("a"));
        subject.OnNext(4);
        subject.OnNext(5);

        list.AssertEqual([]);
        provider.Advance((1));
        list.AssertEqual([]);
        provider.Advance((1));
        list.AssertEqual([]);
        e.Should().BeEmpty();
        provider.Advance((1));

        list.AssertEqual([1, 2, 3, 4, 5]);
        e[0].Message.Should().Be("a");

        list.Clear();

        subject.OnNext(10);
        provider.Advance((1));
        subject.OnNext(20);
        provider.Advance((1));
        subject.OnNext(30);
        provider.Advance((1));

        list.AssertEqual([10]);
        provider.Advance((1));
        list.AssertEqual([10, 20]);
        provider.Advance((1));
        list.AssertEqual([10, 20, 30]);

        subject.OnCompleted();
        list.AssertIsNotCompleted();
        provider.Advance((3));
        list.AssertIsCompleted();
    }
}
