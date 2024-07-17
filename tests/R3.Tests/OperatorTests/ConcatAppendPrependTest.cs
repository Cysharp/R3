namespace R3.Tests.OperatorTests;

public class ConcatAppendPrependTest
{
    [Fact]
    public void Prepend()
    {
        var subject = new Subject<int>();
        using var list = subject.Prepend(9999).ToLiveList();

        subject.OnNext(10);

        list.AssertEqual([9999, 10]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void Prepend2()
    {
        using var list = Observable.Range(1, 3).Prepend(9999).ToLiveList();

        list.AssertEqual([9999, 1, 2, 3]);
    }

    [Fact]
    public void Append()
    {
        var subject = new Subject<int>();
        using var list = subject.Append(9999).ToLiveList();

        subject.OnNext(10);

        list.AssertEqual([10]);

        subject.OnCompleted();

        list.AssertEqual([10, 9999]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void Concat()
    {
        var subject = new Subject<int>();
        var subject2 = new Subject<int>();
        using var list = subject.Concat(subject2).ToLiveList();

        subject.OnNext(10);
        subject2.OnNext(9999);

        list.AssertEqual([10]);

        subject.OnCompleted();

        subject2.OnNext(11111);

        list.AssertEqual([10, 11111]);

        subject2.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatMany()
    {
        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();
        using var list = Observable.Concat(subject1, subject2, subject3).ToLiveList();

        subject1.OnNext(10);
        subject2.OnNext(9999);

        list.AssertEqual([10]);

        subject1.OnCompleted();

        subject2.OnNext(11111);

        list.AssertEqual([10, 11111]);

        subject2.OnCompleted();

        subject3.OnNext(9999999);

        list.AssertEqual([10, 11111, 9999999]);

        subject3.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatNestedSources()
    {
        var outerSubject = new Subject<Observable<int>>();
        using var list = outerSubject.Concat().ToLiveList();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        outerSubject.OnNext(subject1);
        outerSubject.OnNext(subject2);
        outerSubject.OnNext(subject3);

        subject1.OnNext(10);
        subject2.OnNext(9999);

        list.AssertEqual([10]);

        subject1.OnCompleted();

        subject2.OnNext(11111);

        list.AssertEqual([10, 11111]);

        subject2.OnCompleted();

        subject3.OnNext(9999999);

        list.AssertEqual([10, 11111, 9999999]);

        subject3.OnCompleted();

        outerSubject.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatNestedSources_WaitForInner()
    {
        var outerSubject = new Subject<Observable<int>>();
        using var list = outerSubject.Concat().ToLiveList();

        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        outerSubject.OnNext(subject1);
        outerSubject.OnNext(subject2);
        outerSubject.OnNext(subject3);

        subject1.OnNext(10);
        subject2.OnNext(9999);

        list.AssertEqual([10]);

        subject1.OnCompleted();

        subject2.OnNext(11111);

        list.AssertEqual([10, 11111]);

        subject2.OnCompleted();

        subject3.OnNext(9999999);
        outerSubject.OnCompleted();

        list.AssertIsNotCompleted();
        list.AssertEqual([10, 11111, 9999999]);

        subject3.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatNestedSources_Empty()
    {
        var outerSubject = new Subject<Observable<int>>();

        using var list = outerSubject.Concat().ToLiveList();
        list.AssertIsNotCompleted();
        outerSubject.OnCompleted();

        list.AssertIsCompleted();
        list.AssertEqual([]);
    }

    // Prepend factory
    [Fact]
    public void PrependFactory()
    {
        {
            using var list = Observable.Range(1, 3).Prepend(() => 10).ToLiveList();
            list.AssertEqual([10, 1, 2, 3]);
        }
        // with state
        {
            var o = new { V = 20 };
            using var list = Observable.Range(1, 3).Prepend(o, static x => x.V).ToLiveList();
            list.AssertEqual([20, 1, 2, 3]);
        }
    }

    [Fact]
    public void PrependEnumerable()
    {
        // Array
        {
            using var list = Observable.Range(1, 3).Prepend([10, 11, 12]).ToLiveList();
            list.AssertEqual([10, 11, 12, 1, 2, 3]);
        }
        // Pure Enumerable
        {
            using var list = Observable.Range(1, 3).Prepend(Iterate()).ToLiveList();
            list.AssertEqual([100, 200, 300, 1, 2, 3]);
        }
    }

    [Fact]
    public void AppendFactory()
    {
        {
            using var list = Observable.Range(1, 3).Append(() => 10).ToLiveList();
            list.AssertEqual([1, 2, 3, 10]);
        }
        // with state
        {
            var o = new { V = 20 };
            using var list = Observable.Range(1, 3).Append(o, static x => x.V).ToLiveList();
            list.AssertEqual([1, 2, 3, 20]);
        }
    }

    [Fact]
    public void AppendEnumerable()
    {
        // Array
        {
            using var list = Observable.Range(1, 3).Append([10, 11, 12]).ToLiveList();
            list.AssertEqual([1, 2, 3, 10, 11, 12]);
        }
        // Pure Enumerable
        {
            using var list = Observable.Range(1, 3).Append(Iterate()).ToLiveList();
            list.AssertEqual([1, 2, 3, 100, 200, 300]);
        }
    }

    static IEnumerable<int> Iterate()
    {
        yield return 100;
        yield return 200;
        yield return 300;
    }

    [Fact]
    public void ConcatTail()
    {
        var fakeTime = new FakeTimeProvider();

        using var list = Observable.Concat(
                Observable.Return(1), // immediate
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 2), // delay
                Observable.Return(3), // immediate
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 4) // delay
            )
            .ToLiveList();

        list.AssertEqual([1]);

        fakeTime.Advance(1);

        list.AssertEqual([1, 2, 3]);

        fakeTime.Advance(1);

        list.AssertEqual([1, 2, 3, 4]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatAllImmediateTail()
    {
        using var list = Observable.Concat(
                Observable.Return(1), // immediate
                Observable.Return(2),
                Observable.Return(3), // immediate
                Observable.Return(4)
            )
            .ToLiveList();

        list.AssertEqual([1, 2, 3, 4]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void ConcatAllDelay()
    {
        var fakeTime = new FakeTimeProvider();

        using var list = Observable.Concat(
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 1), // delay
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 2), // delay
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 3), // delay
                Observable.Timer(TimeSpan.FromSeconds(1), fakeTime).Select(_ => 4) // delay
            )
            .ToLiveList();

        list.AssertEqual([]);

        fakeTime.Advance(1);
        list.AssertEqual([1]);

        fakeTime.Advance(1);
        list.AssertEqual([1, 2]);

        fakeTime.Advance(1);
        list.AssertEqual([1, 2, 3]);

        fakeTime.Advance(1);
        list.AssertEqual([1, 2, 3, 4]);

        list.AssertIsCompleted();
    }
}
