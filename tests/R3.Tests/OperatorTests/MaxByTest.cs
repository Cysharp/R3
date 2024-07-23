namespace R3.Tests.OperatorTests;

public class MaxByTest
{
    [Fact]
    public async Task First()
    {
        var items = new[] { (3, 1), (2, 2), (1, 3) }.ToObservable();

        var task = items.MaxByAsync(static x => x.Item1);
        (await task).Should().Be((3, 1));
    }

    [Fact]
    public async Task Last()
    {
        var items = new[] { (1, 3), (2, 2), (3, 1) }.ToObservable();

        var task = items.MaxByAsync(static x => x.Item1);
        (await task).Should().Be((3, 1));
    }

    [Fact]
    public async Task Midway()
    {
        var items = new[] { (2, 2), (3, 1), (1, 3) }.ToObservable();

        var task = items.MaxByAsync(static x => x.Item1);
        (await task).Should().Be((3, 1));
    }
}
