namespace R3.Tests.OperatorTests;

public class ZipTest
{
    [Fact]
    public void Zip()
    {
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

        list.AssertIsCompleted();
    }


    [Fact]
    public void ZipCompletedCheck1()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual((1, 10, 100));

        source2.OnNext(20);
        source3.OnNext(200);
        source1.OnCompleted(); // no queue oncompleted immediately

        list.AssertIsCompleted();
    }

    [Fact]
    public void ZipCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual((1, 10, 100));

        source1.OnNext(2);
        source1.OnNext(3);
        source1.OnCompleted(); // still exists in queue

        list.AssertIsNotCompleted();

        source2.OnNext(20);
        source2.OnNext(30);
        source2.OnNext(40);

        source3.OnNext(200);
        list.AssertEqual((1, 10, 100), (2, 20, 200));

        source3.OnNext(300);
        list.AssertEqual((1, 10, 100), (2, 20, 200), (3, 30, 300)); // source1 queue is empty, call complete

        list.AssertIsCompleted();
    }

    [Fact]
    public void ZipCompletedCheck3()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        
        source1.OnCompleted(); // still exists in queue but completed all
        source2.OnCompleted();
        source3.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void NthZipCompletedCheck1()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        source2.OnNext(20);
        source3.OnNext(200);
        source1.OnCompleted(); // no queue oncompleted immediately

        list.AssertIsCompleted();
    }

    [Fact]
    public void NthZipCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        source1.OnNext(2);
        source1.OnNext(3);
        source1.OnCompleted(); // still exists in queue

        list.AssertIsNotCompleted();

        source2.OnNext(20);
        source2.OnNext(30);
        source2.OnNext(40);

        source3.OnNext(200);
        list.AssertEqual([1, 10, 100], [2, 20, 200]);

        source3.OnNext(300);
        list.AssertEqual([1, 10, 100], [2, 20, 200], [3, 30, 300]); // source1 queue is empty, call complete

        list.AssertIsCompleted();
    }

    [Fact]
    public void NthZipCompletedCheck3()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.Zip(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);

        source1.OnCompleted(); // still exists in queue but completed all
        source2.OnCompleted();
        source3.OnCompleted();

        list.AssertIsCompleted();
    }
}
