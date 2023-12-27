namespace R3.Tests.OperatorTests;

public class IgnoreElementsTest
{
    [Fact]
    public void Test()
    {
        var subject = new Subject<int>();
        using var list = subject.IgnoreElements().ToLiveList();

        subject.OnNext(10);
        subject.OnNext(20);
        subject.OnNext(30);

        list.AssertEqual([]);
        list.AssertIsNotCompleted();

        subject.OnCompleted();

        list.AssertIsCompleted();
    }
}
