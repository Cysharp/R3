namespace R3.Tests.OperatorTests;

public class ZipLatestTest
{
    [Fact]
    public void ZipLatest()
    {
        // 2つペアになった時点で新しい方から吐き出されていく
        // 新しい要素が入ってきたら古いものは捨てられる

        var source1 = new Subject<int>();
        var source2 = new Subject<string>();

        using var list = source1.ZipLatest(source2, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);

        list.AssertEmpty();

        source1.OnNext(2);

        list.AssertEmpty();

        source2.OnNext("a");

        list.AssertEqual([(2, "a")]);

        source1.OnNext(3);

        list.AssertEqual([(2, "a")]);

        source2.OnNext("b");

        list.AssertEqual([(2, "a"), (3, "b")]);

        source2.OnNext("c");

        list.AssertEqual([(2, "a"), (3, "b")]);

        source1.OnCompleted();

        list.AssertIsNotCompleted();

        source1.OnNext(4);
        source2.OnNext("d");

        list.AssertEqual([(2, "a"), (3, "b")]);

        source2.OnCompleted();

        list.AssertIsCompleted();
    }
}
