namespace R3.Tests.OperatorTests;

public class MaxByTest
{
    [Fact]
    public async Task MaxBy()
    {
        var items = new[] { (1, 3), (2, 2), (3, 1) }.ToObservable();

        var task = items.MaxByAsync(static x => x.Item1);
        (await task).Should().Be((3, 1));
    }
}
