namespace R3.Tests.OperatorTests;

public class IsEmptyTest
{
    [Fact]
    public async Task Positive()
    {
        var range = Observable.Empty<int>();
        var task = range.IsEmptyAsync();
        (await task).Should().BeTrue();
    }

    [Fact]
    public async Task Negative()
    {
        var range = Observable.Return(0);
        var task = range.IsEmptyAsync();
        (await task).Should().BeFalse();
    }
}
