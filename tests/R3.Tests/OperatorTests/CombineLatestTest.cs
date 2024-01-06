namespace R3.Tests.OperatorTests;

public class CombineLatestTest
{
    [Fact]
    public void CombineLatest()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<string>();

        using var list = source1.CombineLatest(source2, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);

        list.AssertEmpty();

        source1.OnNext(2);

        list.AssertEmpty();

        source2.OnNext("a");

        list.AssertEqual([(2, "a")]);

        source1.OnNext(3);

        list.AssertEqual([(2, "a"), (3, "a")]);

        source2.OnNext("b");

        list.AssertEqual([(2, "a"), (3, "a"), (3, "b")]);

        source2.OnNext("c");

        list.AssertEqual([(2, "a"), (3, "a"), (3, "b"), (3, "c")]);

        source1.OnCompleted();

        list.AssertIsNotCompleted();

        source1.OnNext(4);
        source2.OnNext("d");

        list.AssertEqual([(2, "a"), (3, "a"), (3, "b"), (3, "c"), (3, "d")]);

        source2.OnCompleted();

        list.AssertIsCompleted();
    }
}
