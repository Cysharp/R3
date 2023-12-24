namespace R3.Tests.OperatorTests;

public class WhereTest(ITestOutputHelper output)
{
    // test WhereWhere optimize
    [Fact]
    public void WhereWhere()
    {
        var p = new Subject<int>();

        using var list = p.Where(x => x % 2 != 0).Where(x => x % 3 != 0).ToLiveList();

        p.OnNext(2);
        list.AssertEqual([]);

        p.OnNext(1);
        list.AssertEqual([1]);

        p.OnNext(3);
        list.AssertEqual([1]);

        p.OnNext(5);
        list.AssertEqual([1, 5]);

        p.OnNext(6);
        list.AssertEqual([1, 5]);

        p.OnNext(7);
        list.AssertEqual([1, 5, 7]);
    }


    // test where completable
    [Fact]
    public void WhereCompletable()
    {
        var p = new Subject<int>();

        using var list = p.Where(x => x % 2 != 0).ToLiveList();

        p.OnNext(2);
        list.AssertEqual([]);

        p.OnNext(1);
        list.AssertEqual([1]);

        p.OnNext(3);
        list.AssertEqual([1, 3]);

        p.OnNext(30);
        list.AssertEqual([1, 3]);

        list.AssertIsNotCompleted();

        p.OnCompleted(default);

        list.AssertIsCompleted();
    }


    // test where completable indexed
    [Fact]
    public void WhereCompletableIndexed()
    {
        var p = new Subject<int>();

        using var list = p.Where((x, i) => i % 2 != 0).ToLiveList();

        p.OnNext(2);
        list.AssertEqual([]);

        p.OnNext(1);
        list.AssertEqual([1]);

        p.OnNext(3);
        list.AssertEqual([1]);

        p.OnNext(5);
        list.AssertEqual([1, 5]);

        p.OnNext(6);
        list.AssertEqual([1, 5]);

        p.OnNext(8);
        list.AssertEqual([1, 5, 8]);

        list.AssertIsNotCompleted();

        p.OnCompleted(default);

        list.AssertIsCompleted();
    }

    // test where with state
    [Fact]
    public void WhereState()
    {
        var p = new Subject<int>();

        var state = new { x = 2, y = 0 };
        using var list = p.Where(state, static (x, s) => x % s.x != s.y).ToLiveList();

        p.OnNext(2);
        list.AssertEqual([]);

        p.OnNext(1);
        list.AssertEqual([1]);

        p.OnNext(3);
        list.AssertEqual([1, 3]);

        p.OnNext(30);
        list.AssertEqual([1, 3]);

        list.AssertIsNotCompleted();

        p.OnCompleted(default);

        list.AssertIsCompleted();
    }
}
