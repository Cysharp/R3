using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class SkipWhileTest
{
    [Fact]
    public void SkipWhile()
    {
        var xs = Observable.Range(1, 10).SkipWhile(x => x <= 3).ToLiveList();
        xs.AssertEqual([4, 5, 6, 7, 8, 9, 10]);
        xs.AssertIsCompleted();

        var ys = Observable.Range(100, 10).SkipWhile((x, i) => i < 3).ToLiveList();
        ys.AssertEqual([103, 104, 105, 106, 107, 108, 109]);
        ys.AssertIsCompleted();
    }
}
