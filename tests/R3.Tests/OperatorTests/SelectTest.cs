namespace R3.Tests.OperatorTests;

public class SelectTest
{
    [Fact]
    public void Select()
    {
        var subject = new Subject<int>();
        using var list = subject.Select(x => x * 2).ToLiveList();

        subject.OnNext(10);
        list.AssertEqual([20]);

        subject.OnNext(20);
        list.AssertEqual([20, 40]);

        subject.OnNext(40);
        list.AssertEqual([20, 40, 80]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }

    // WhereSelect
    [Fact]
    public void WhereSelect()
    {
        var subject = new Subject<int>();
        using var list = subject.Where(x => x % 2 == 0).Select(x => x * 2).ToLiveList();

        subject.OnNext(10);
        list.AssertEqual([20]);

        subject.OnNext(11);
        list.AssertEqual([20]);

        subject.OnNext(20);
        list.AssertEqual([20, 40]);

        subject.OnNext(40);
        list.AssertEqual([20, 40, 80]);

        subject.OnNext(99);
        list.AssertEqual([20, 40, 80]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }

    // SelectWithIndex
    [Fact]
    public void SelectWithIndex()
    {
        var subject = new Subject<int>();
        using var list = subject.Select((x, i) => x * 2 + i).ToLiveList();

        subject.OnNext(10);
        list.AssertEqual([20]);

        subject.OnNext(20);
        list.AssertEqual([20, 41]);

        subject.OnNext(40);
        list.AssertEqual([20, 41, 82]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }

    // Select State
    [Fact]
    public void SelectState()
    {
        var subject = new Subject<int>();
        using var list = subject.Select("a", (x, state) => x * 2 + state).ToLiveList();

        subject.OnNext(10);
        list.AssertEqual(["20a"]);

        subject.OnNext(20);
        list.AssertEqual(["20a", "40a"]);

        subject.OnNext(40);
        list.AssertEqual(["20a", "40a", "80a"]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }
}
