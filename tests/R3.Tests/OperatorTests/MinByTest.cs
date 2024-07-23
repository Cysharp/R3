namespace R3.Tests.OperatorTests;

public class MinByTest
{
    [Fact]
    public async Task First()
    {
        var items = new[] { (1, 3), (2, 2), (3, 1) }.ToObservable();

        var task = items.MinByAsync(static x => x.Item1);
        (await task).Should().Be((1, 3));
    }

    [Fact]
    public async Task Last()
    {
        var items = new[] { (3, 1), (2, 2), (1, 3) }.ToObservable();

        var task = items.MinByAsync(static x => x.Item1);
        (await task).Should().Be((1, 3));
    }

    [Fact]
    public async Task Midway()
    {
        var items = new[] { (2, 2), (1, 3), (3, 1) }.ToObservable();

        var task = items.MinByAsync(static x => x.Item1);
        (await task).Should().Be((1, 3));
    }
}
