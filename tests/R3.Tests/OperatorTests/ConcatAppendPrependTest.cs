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
}
