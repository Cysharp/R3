using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class TakeWhileTest
{
    [Fact]
    public void TakeWhile()
    {
        var xs = Observable.Range(1, 10).TakeWhile(x => x <= 3).ToLiveList();
        xs.AssertEqual([1, 2, 3]);
        xs.AssertIsCompleted();

        var ys = Observable.Range(100, 10).TakeWhile((x, i) => i < 3).ToLiveList();
        ys.AssertEqual([100, 101, 102]);
        ys.AssertIsCompleted();
    }
}
