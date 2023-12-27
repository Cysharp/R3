namespace R3.Tests.OperatorTests;

public class CastTest
{
    [Fact]
    public void Cast()
    {
        var subject = new Subject<object>();
        using var list = subject.Cast<object, int>().ToLiveList();

        subject.OnNext(10);
        subject.OnNext(20);
        subject.OnNext(30);

        list.AssertEqual([10, 20, 30]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }
}
