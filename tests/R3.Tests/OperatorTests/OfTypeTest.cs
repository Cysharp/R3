namespace R3.Tests.OperatorTests;

public class OfTypeTest
{
    [Fact]
    public void Test()
    {
        var subject = new Subject<object>();
        using var list = subject.OfType<object, int>().ToLiveList();

        subject.OnNext(10);
        subject.OnNext("hello");
        subject.OnNext(20);
        subject.OnNext(30);
        subject.OnNext("world");
        subject.OnNext(40);

        list.AssertEqual([10, 20, 30, 40]);

        subject.OnCompleted();
        list.AssertIsCompleted();
    }
}
