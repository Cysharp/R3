namespace R3.Tests.OperatorTests;

public class AnyTest
{
    [Fact]
    public async Task Positive()
    {
        var range = Observable.Range(1, 10);
        var task = range.AnyAsync(static x => x is 5);
        (await task).Should().BeTrue();
    }

    [Fact]
    public async Task Negative()
    {
        var range = Observable.Range(1, 10);
        var task = range.AnyAsync(static x => x is 11);
        (await task).Should().BeFalse();
    }
}
