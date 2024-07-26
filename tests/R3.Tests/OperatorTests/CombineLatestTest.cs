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

    [Fact]
    public void CombineLatestCompletedCheck()
    {
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<int>();
            var source3 = new Subject<int>();
            using var list = Observable.CombineLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

            // no value cached complete
            source1.OnCompleted();

            list.AssertIsCompleted();
        }
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<int>();
            var source3 = new Subject<int>();
            using var list = Observable.CombineLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

            // value cached, ok
            source1.OnNext(1);
            source1.OnCompleted();

            list.AssertIsNotCompleted();
        }
    }

    [Fact]
    public void CombineLatestCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.CombineLatest(source1, source2, source3, ValueTuple.Create).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);

        list.AssertEqual((1, 10, 100));

        source1.OnCompleted();
        list.AssertIsNotCompleted();

        source2.OnCompleted();
        list.AssertIsNotCompleted();

        source3.OnNext(200);
        list.AssertEqual((1, 10, 100), (1, 10, 200));

        source3.OnCompleted();
        list.AssertIsCompleted(); // all completed
    }

    [Fact]
    public void NthCombineLatestCompletedCheck()
    {
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<int>();
            var source3 = new Subject<int>();
            using var list = Observable.CombineLatest(source1, source2, source3).ToLiveList();

            // no value cached complete
            source1.OnCompleted();

            list.AssertIsCompleted();
        }
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<int>();
            var source3 = new Subject<int>();
            using var list = Observable.CombineLatest(source1, source2, source3).ToLiveList();

            // value cached, ok
            source1.OnNext(1);
            source1.OnCompleted();

            list.AssertIsNotCompleted();
        }
    }

    [Fact]
    public void NthCombineLatestCompletedCheck2()
    {
        var source1 = new Subject<int>();
        var source2 = new Subject<int>();
        var source3 = new Subject<int>();

        using var list = Observable.CombineLatest(source1, source2, source3).ToLiveList();

        source1.OnNext(1);
        source2.OnNext(10);
        source3.OnNext(100);

        list.AssertEqual([1, 10, 100]);

        source1.OnCompleted();
        list.AssertIsNotCompleted();

        source2.OnCompleted();
        list.AssertIsNotCompleted();

        source3.OnNext(200);
        list.AssertEqual([1, 10, 100], [1, 10, 200]);

        source3.OnCompleted();
        list.AssertIsCompleted(); // all completed
    }
}
