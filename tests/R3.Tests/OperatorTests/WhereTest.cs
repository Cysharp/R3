namespace R3.Tests.OperatorTests;

public class WhereTest(ITestOutputHelper output)
{
    [Fact]
    public void Where()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1, 3]);

        p.PublishOnNext(30);
        list.AssertEqual([1, 3]);
    }

    // test WhereWhere optimize
    [Fact]
    public void WhereWhere()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).Where(x => x % 3 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1]);

        p.PublishOnNext(5);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(6);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(7);
        list.AssertEqual([1, 5, 7]);
    }

    //test where indexed
    [Fact]
    public void WhereIndexed()
    {
        var p = new Publisher<int>();

        using var list = p.Where((x, i) => i % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1]);

        p.PublishOnNext(5);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(6);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(8);
        list.AssertEqual([1, 5, 8]);
    }

    // test where completable
    [Fact]
    public void WhereCompletable()
    {
        var p = new CompletablePublisher<int, Unit>();

        using var list = p.Where(x => x % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1, 3]);

        p.PublishOnNext(30);
        list.AssertEqual([1, 3]);

        list.AssertIsNotCompleted();

        p.PublishOnCompleted(default);

        list.AssertIsCompleted();
    }


    // test where completable indexed
    [Fact]
    public void WhereCompletableIndexed()
    {
        var p = new CompletablePublisher<int, Unit>();

        using var list = p.Where((x, i) => i % 2 != 0).LiveRecord();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1]);

        p.PublishOnNext(5);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(6);
        list.AssertEqual([1, 5]);

        p.PublishOnNext(8);
        list.AssertEqual([1, 5, 8]);

        list.AssertIsNotCompleted();

        p.PublishOnCompleted(default);

        list.AssertIsCompleted();
    }
}
