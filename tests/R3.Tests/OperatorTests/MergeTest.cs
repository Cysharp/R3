namespace R3.Tests.OperatorTests;

public class MergeTest
{
    [Fact]
    public void Std()
    {
        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        using var list = Observable.Merge(subject1, subject2, subject3).ToLiveList();

        subject1.OnNext(10);
        subject2.OnNext(20);
        subject2.OnNext(21);
        subject3.OnNext(30);
        subject1.OnNext(11);
        subject3.OnNext(31);

        subject2.OnCompleted();

        subject1.OnNext(12);
        subject1.OnCompleted();

        subject3.OnNext(32);

        list.AssertIsNotCompleted();

        subject3.OnCompleted();

        list.AssertEqual([10, 20, 21, 30, 11, 31, 12, 32]);

        list.AssertIsCompleted();
    }

    [Fact]
    public void CompleteFirst()
    {
        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        subject1.OnCompleted();
        subject2.OnCompleted();
        subject3.OnCompleted();
        using var list = Observable.Merge(subject1, subject2, subject3).ToLiveList();

        list.AssertIsCompleted();
    }

    [Fact]
    public void Empty()
    {
        using var list = Observable.Empty<int>().ToLiveList();

        list.AssertIsCompleted();
    }

    [Fact]
    public void NestedSources()
    {
        var outerSubject = new Subject<Observable<int>>();

        using var list = Observable.Merge(outerSubject).ToLiveList();

        var innerSubject1 = new Subject<int>();
        var innerSubject2 = new Subject<int>();
        var innerSubject3 = new Subject<int>();

        outerSubject.OnNext(innerSubject1);
        innerSubject1.OnNext(10);
        innerSubject1.OnNext(11);

        outerSubject.OnNext(innerSubject2);
        innerSubject2.OnNext(20);
        innerSubject1.OnNext(12);
        innerSubject2.OnNext(21);

        outerSubject.OnNext(innerSubject3);
        innerSubject3.OnNext(30);
        innerSubject2.OnNext(22);
        innerSubject1.OnNext(12);

        innerSubject1.OnCompleted();
        innerSubject2.OnCompleted();
        innerSubject3.OnCompleted();

        list.AssertIsNotCompleted();

        outerSubject.OnCompleted();

        list.AssertIsCompleted();
        list.AssertEqual([10, 11, 20, 12, 21, 30, 22, 12]);
    }

    [Fact]
    public void NestedSources_WaitForInners()
    {
        var outerSubject = new Subject<Observable<int>>();

        using var list = Observable.Merge(outerSubject).ToLiveList();

        var innerSubject1 = new Subject<int>();
        var innerSubject2 = new Subject<int>();
        var innerSubject3 = new Subject<int>();

        outerSubject.OnNext(innerSubject1);
        innerSubject1.OnNext(10);
        innerSubject1.OnNext(11);

        outerSubject.OnNext(innerSubject2);
        innerSubject2.OnNext(20);
        innerSubject1.OnNext(12);
        innerSubject2.OnNext(21);

        outerSubject.OnNext(innerSubject3);
        innerSubject3.OnNext(30);
        innerSubject2.OnNext(22);
        innerSubject1.OnNext(12);

        innerSubject1.OnCompleted();
        innerSubject3.OnCompleted();
        outerSubject.OnCompleted();

        list.AssertIsNotCompleted();

        innerSubject2.OnCompleted();

        list.AssertIsCompleted();
        list.AssertEqual([10, 11, 20, 12, 21, 30, 22, 12]);
    }

    [Fact]
    public void NestedSources_Empty()
    {
        var outerSubject = new Subject<Observable<int>>();

        using var list = Observable.Merge(outerSubject).ToLiveList();
        list.AssertIsNotCompleted();
        outerSubject.OnCompleted();

        list.AssertIsCompleted();
        list.AssertEqual([]);
    }
}
