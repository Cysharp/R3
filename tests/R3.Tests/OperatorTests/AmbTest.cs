namespace R3.Tests.OperatorTests;

public class AmbTest
{
    [Fact]
    public void Amb3()
    {
        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        var d1Called = false;
        var d2Called = false;
        var d3Called = false;

        var list = Observable.Amb(
            subject1.Do(onDispose: () => d1Called = true),
            subject2.Do(onDispose: () => d2Called = true),
            subject3.Do(onDispose: () => d3Called = true)
            ).ToLiveList();

        d1Called.Should().BeFalse();

        subject2.OnNext(2);
        list.AssertEqual([2]);

        d1Called.Should().BeTrue();
        d3Called.Should().BeTrue();

        subject2.OnNext(20);
        list.AssertEqual([2, 20]);

        subject2.OnCompleted();

        d2Called.Should().BeTrue();
        list.AssertIsCompleted();
    }
}
