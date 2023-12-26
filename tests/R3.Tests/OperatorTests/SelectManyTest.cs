namespace R3.Tests.OperatorTests;

public class SelectManyTest
{
    [Fact]
    public void Standard()
    {
        var root = new Subject<int>();
        var child1 = new Subject<int>();
        var child2 = new Subject<int>();
        var child3 = new Subject<int>();
        Subject<int>[] childs = [child1, child2, child3];

        using var list = root.SelectMany(x => childs[x], (x, y) => y).ToLiveList();

        // many
        root.OnNext(0);
        root.OnNext(1);
        root.OnNext(2);

        list.AssertEqual([]);

        child1.OnNext(100);
        list.AssertEqual([100]);

        child2.OnNext(2000);
        list.AssertEqual([100, 2000]);

        child3.OnCompleted();
        child1.OnCompleted();
        child2.OnCompleted();

        list.AssertIsNotCompleted();

        root.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void Standard2()
    {
        var root = new Subject<int>();
        var child1 = new Subject<int>();
        var child2 = new Subject<int>();
        var child3 = new Subject<int>();
        Subject<int>[] childs = [child1, child2, child3];

        using var list = root.SelectMany(x => childs[x], (x, y) => y).ToLiveList();

        // many
        root.OnNext(0);
        root.OnNext(1);
        root.OnNext(2);

        list.AssertEqual([]);

        child1.OnNext(100);
        list.AssertEqual([100]);

        child2.OnNext(2000);
        list.AssertEqual([100, 2000]);


        root.OnCompleted();
        list.AssertIsNotCompleted();

        child1.OnCompleted();
        child2.OnCompleted();

        list.AssertIsNotCompleted();

        child3.OnNext(9999);
        list.AssertEqual([100, 2000, 9999]);

        child3.OnCompleted();
        list.AssertIsCompleted();
    }


    [Fact]
    public void Error()
    {
        var root = new Subject<int>();
        var child1 = new Subject<int>();
        var child2 = new Subject<int>();
        var child3 = new Subject<int>();
        Subject<int>[] childs = [child1, child2, child3];

        using var list = root.SelectMany(x => childs[x], (x, y) => y).ToLiveList();

        // many
        root.OnNext(0);
        root.OnNext(1);
        root.OnNext(2);

        list.AssertEqual([]);

        child1.OnNext(100);
        list.AssertEqual([100]);

        child2.OnNext(2000);
        list.AssertEqual([100, 2000]);

        child3.OnCompleted(Result.Failure(new Exception()));
        list.AssertIsCompleted();
    }


    [Fact]
    public void WithIndex()
    {
        var root = new Subject<int>();
        var child1 = new Subject<int>();
        var child2 = new Subject<int>();
        var child3 = new Subject<int>();
        Subject<int>[] childs = [child1, child2, child3];

        using var list = root.SelectMany((x, i) => childs[i], (x, i, y, i2) => (x, i, y, i2)).ToLiveList();

        // many
        root.OnNext(1000);
        root.OnNext(2000);
        root.OnNext(3000);

        list.AssertEqual([]);

        child1.OnNext(100);
        list.AssertEqual([(1000, 0, 100, 0)]);
        child1.OnNext(200);
        list.AssertEqual([(1000, 0, 100, 0), (1000, 0, 200, 1)]);

        child2.OnNext(300);
        list.AssertEqual([(1000, 0, 100, 0), (1000, 0, 200, 1), (2000, 1, 300, 0)]);

        child3.OnCompleted();
        child1.OnCompleted();
        child2.OnCompleted();

        list.AssertIsNotCompleted();

        root.OnCompleted();
        list.AssertIsCompleted();
    }
}
