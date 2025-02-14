namespace R3.Tests.OperatorTests;

public class ForEachAsyncTest
{
    [Fact]
    public async Task ForEach()
    {
        var range = Observable.Range(1, 10);

        {
            var l = new List<int>();
            await range.ForEachAsync(l.Add);
            l.ShouldBe([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        }
        {
            var l = new List<(int, int)>();
            await range.ForEachAsync((x, i) => l.Add((x, i)));
            l.Select(x => x.Item1).ShouldBe([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
            l.Select(x => x.Item2).ShouldBe([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        }
    }
}
