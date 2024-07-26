namespace R3.Tests.OperatorTests;

public class ZipLatestTest
{
    [Fact]
    public void ZipLatest()
    {
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

        list.AssertIsCompleted();
    }


    [Fact]
    public void ZipLatestCompletedCheck1()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual((1, 10, 100));

        source2.OnNext(20);
        source3.OnNext(200);
        source1.OnCompleted(); // no latest oncompleted immediately

        list.AssertIsCompleted();
    }

    [Fact]
    public void ZipLatestCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual((1, 10, 100));

        source1.OnNext(2);
        source1.OnNext(3);
        source1.OnCompleted(); // still exists value(3)

        list.AssertIsNotCompleted();

        source2.OnNext(20);
        source2.OnNext(30);
        source2.OnNext(40); // 40

        source3.OnNext(200); // 200 and flush, no more value
        list.AssertEqual((1, 10, 100), (3, 40, 200));

        list.AssertIsCompleted();
    }

    [Fact]
    public void ZipLatestCompletedCheck3()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);

        source1.OnCompleted(); // still exists values but completed all
        source2.OnCompleted();
        source3.OnCompleted();

        list.AssertIsCompleted();
    }



    [Fact]
    public void NthZipLatestCompletedCheck1()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        source2.OnNext(20);
        source3.OnNext(200);
        source1.OnCompleted(); // no latest oncompleted immediately

        list.AssertIsCompleted();
    }

    [Fact]
    public void NthZipLatestCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        source1.OnNext(2);
        source1.OnNext(3);
        source1.OnCompleted(); // still exists value(3)

        list.AssertIsNotCompleted();

        source2.OnNext(20);
        source2.OnNext(30);
        source2.OnNext(40); // 40

        source3.OnNext(200); // 200 and flush, no more value
        list.AssertEqual([1, 10, 100], [3, 40, 200]);

        list.AssertIsCompleted();
    }

    [Fact]
    public void NthZipLatestCompletedCheck3()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.ZipLatest(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);

        source1.OnCompleted(); // still exists values but completed all
        source2.OnCompleted();
        source3.OnCompleted();

        list.AssertIsCompleted();
    }
}
