namespace R3.Tests.OperatorTests;

public class AllTest
{
    [Fact]
    public async Task Positive()
    {
        var range = Observable.Range(1, 10);
        var task = range.AllAsync(static x => x is > 0 and <= 10);
        (await task).Should().BeTrue();
    }

    [Fact]
    public async Task Negative()
    {
        var range = Observable.Range(1, 10);
        var task = range.AllAsync(static x => x is > 0 and < 10);
        (await task).Should().BeFalse();
    }
}
