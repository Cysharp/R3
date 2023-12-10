using FluentAssertions;

namespace R3.Tests;

public class WhereTest(ITestOutputHelper output)
{
    [Fact]
    public void Where()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.Should().BeEmpty();

        p.PublishOnNext(1);
        list.Should().Equal([1]);

        p.PublishOnNext(3);
        list.Should().Equal([1, 3]);

        p.PublishOnNext(30);
        list.Should().Equal([1, 3]);
    }

    // test WhereWhere optimize
    [Fact]
    public void WhereWhere()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).Where(x => x % 3 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.Should().BeEmpty();

        p.PublishOnNext(1);
        list.Should().Equal([1]);

        p.PublishOnNext(3);
        list.Should().Equal([1]);

        p.PublishOnNext(5);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(6);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(7);
        list.Should().Equal([1, 5, 7]);
    }

    //test where indexed
    [Fact]
    public void WhereIndexed()
    {
        var p = new Publisher<int>();

        using var list = p.Where((x, i) => i % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.Should().BeEmpty();

        p.PublishOnNext(1);
        list.Should().Equal([1]);

        p.PublishOnNext(3);
        list.Should().Equal([1]);

        p.PublishOnNext(5);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(6);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(8);
        list.Should().Equal([1, 5, 8]);
    }

    // test where completable
    [Fact]
    public void WhereCompletable()
    {
        var p = new CompletablePublisher<int, Unit>();

        using var list = p.Where(x => x % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.Should().BeEmpty();

        p.PublishOnNext(1);
        list.Should().Equal([1]);

        p.PublishOnNext(3);
        list.Should().Equal([1, 3]);

        p.PublishOnNext(30);
        list.Should().Equal([1, 3]);

        list.IsCompleted.Should().BeFalse();

        p.PublishOnCompleted(default);

        list.IsCompleted.Should().BeTrue();
    }


    // test where completable indexed
    [Fact]
    public void WhereCompletableIndexed()
    {
        var p = new CompletablePublisher<int, Unit>();

        using var list = p.Where((x, i) => i % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.Should().BeEmpty();

        p.PublishOnNext(1);
        list.Should().Equal([1]);

        p.PublishOnNext(3);
        list.Should().Equal([1]);

        p.PublishOnNext(5);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(6);
        list.Should().Equal([1, 5]);

        p.PublishOnNext(8);
        list.Should().Equal([1, 5, 8]);

        list.IsCompleted.Should().BeFalse();

        p.PublishOnCompleted(default);

        list.IsCompleted.Should().BeTrue();
    }
}
