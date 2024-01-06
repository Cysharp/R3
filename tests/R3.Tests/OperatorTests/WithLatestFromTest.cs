namespace R3.Tests.OperatorTests;

public class WithLatestFromTest
{
    [Fact]
    public void WithLatestFrom()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<string>();

        using var list = source1.WithLatestFrom(source2, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);

        list.AssertEmpty();

        source1.OnNext(2);

        list.AssertEmpty();

        source2.OnNext("a");

        list.AssertEmpty();

        source1.OnNext(3);

        list.AssertEqual([(3, "a")]);

        source2.OnNext("b");

        list.AssertEqual([(3, "a")]);

        source2.OnNext("c");

        list.AssertEqual([(3, "a")]);

        source1.OnCompleted();

        list.AssertIsCompleted();
    }
}
