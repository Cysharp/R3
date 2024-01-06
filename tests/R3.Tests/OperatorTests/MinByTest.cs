namespace R3.Tests.OperatorTests;

public class MinByTest
{
    [Fact]
    public async Task MinBy()
    {
        var items = new[] { (1, 3), (2, 2), (3, 1) }.ToObservable();

        var task = items.MinByAsync(static x => x.Item1);
        (await task).Should().Be((1, 3));
    }
}
