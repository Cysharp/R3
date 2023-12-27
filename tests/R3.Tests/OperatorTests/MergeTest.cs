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
}
