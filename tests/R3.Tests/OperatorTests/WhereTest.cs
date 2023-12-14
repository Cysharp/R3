namespace R3.Tests.OperatorTests;

public class WhereTest(ITestOutputHelper output)
{
    // test WhereWhere optimize
    [Fact]
    public void WhereWhere()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).Where(x => x % 3 != 0).ToLiveList();

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


    // test where completable
    [Fact]
    public void WhereCompletable()
    {
        var p = new Publisher<int>();

        using var list = p.Where(x => x % 2 != 0).ToLiveList();

        p.PublishOnNext(2);
        list.AssertEqual([]);

        p.PublishOnNext(1);
        list.AssertEqual([1]);

        p.PublishOnNext(3);
        list.AssertEqual([1, 3]);

        p.PublishOnNext(30);
        list.AssertEqual([1, 3]);

        list.AssertIsNoResultd();

        p.PublishOnCompleted(default);

        list.AssertIsCompleted();
    }


    // test where completable indexed
    [Fact]
    public void WhereCompletableIndexed()
    {
        var p = new Publisher<int>();

        using var list = p.Where((x, i) => i % 2 != 0).ToLiveList();

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

        list.AssertIsNoResultd();

        p.PublishOnCompleted(default);

        list.AssertIsCompleted();
    }
}
