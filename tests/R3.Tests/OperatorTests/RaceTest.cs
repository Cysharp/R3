namespace R3.Tests.OperatorTests;

public class RaceTest
{
    [Fact]
    public void Race3()
    {
        var subject1 = new Subject<int>();
        var subject2 = new Subject<int>();
        var subject3 = new Subject<int>();

        var d1Called = false;
        var d2Called = false;
        var d3Called = false;

        var list = Observable.Race(
            subject1.Do(onDispose: () => d1Called = true),
            subject2.Do(onDispose: () => d2Called = true),
            subject3.Do(onDispose: () => d3Called = true)
            ).ToLiveList();

        d1Called.ShouldBeFalse();

        subject2.OnNext(2);
        list.AssertEqual([2]);

        d1Called.ShouldBeTrue();
        d3Called.ShouldBeTrue();

        subject2.OnNext(20);
        list.AssertEqual([2, 20]);

        subject2.OnCompleted();

        d2Called.ShouldBeTrue();
        list.AssertIsCompleted();
    }
}
