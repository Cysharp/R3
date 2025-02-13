namespace R3.Tests.OperatorTests;

public class ContainsTest
{
    [Fact]
    public async Task Positive()
    {
        var range = Observable.Range(1, 10);
        var task = range.ContainsAsync(5);
        (await task).ShouldBeTrue();
    }

    [Fact]
    public async Task Negative()
    {
        var range = Observable.Range(1, 10);
        var task = range.ContainsAsync(0);
        (await task).ShouldBeFalse();
    }
}
