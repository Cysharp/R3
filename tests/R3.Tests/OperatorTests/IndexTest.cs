namespace R3.Tests.OperatorTests;

public class IndexTest
{
    [Fact]
    public void Unit()
    {
        var subject = new Subject<Unit>();
        using var list = subject.Index().ToLiveList();

        subject.OnNext(default);
        subject.OnNext(default);
        subject.OnNext(default);
        subject.OnNext(default);
        subject.OnNext(default);

        list.AssertEqual([0, 1, 2, 3, 4]);

        subject.Dispose();

        list.AssertIsCompleted();
    }

    [Fact]
    public void Item()
    {
        var subject = new Subject<string>();
        using var list = subject.Index().ToLiveList();

        subject.OnNext("a");
        subject.OnNext("b");
        subject.OnNext("c");
        subject.OnNext("d");
        subject.OnNext("e");

        list.AssertEqual([(0, "a"), (1, "b"), (2, "c"), (3, "d"), (4, "e")]);

        subject.Dispose();

        list.AssertIsCompleted();
    }
}
