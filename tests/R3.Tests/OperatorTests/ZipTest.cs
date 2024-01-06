namespace R3.Tests.OperatorTests;

public class ZipTest
{
    [Fact]
    public void Zip()
    {
        // 2つペアになった時点で古いほうから吐き出されていく

        var source1 = new Subject<int>();
        var source2 = new Subject<string>();

        using var list = source1.Zip(source2, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);

        list.AssertEmpty();

        source1.OnNext(2);

        list.AssertEmpty();

        source2.OnNext("a");

        list.AssertEqual([(1, "a")]);

        source1.OnNext(3);

        list.AssertEqual([(1, "a")]);

        source2.OnNext("b");

        list.AssertEqual([(1, "a"), (2, "b")]);

        source2.OnNext("c");

        list.AssertEqual([(1, "a"), (2, "b"), (3, "c")]);

        source1.OnCompleted();

        list.AssertIsNotCompleted();

        source1.OnNext(4);
        source2.OnNext("d");

        list.AssertEqual([(1, "a"), (2, "b"), (3, "c")]);

        source2.OnCompleted();

        list.AssertIsCompleted();
    }
}
