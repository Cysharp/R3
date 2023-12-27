namespace R3.Tests.OperatorTests;

public class AsUnitObservableTest
{
    [Fact]
    public void Test()
    {
        var subject = new Subject<int>();
        using var list = subject.AsUnitObservable().ToLiveList();

        subject.OnNext(10);
        subject.OnNext(20);

        list.AssertEqual([Unit.Default, Unit.Default]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }
}
