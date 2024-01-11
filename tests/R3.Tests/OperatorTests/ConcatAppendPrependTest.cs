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
}
