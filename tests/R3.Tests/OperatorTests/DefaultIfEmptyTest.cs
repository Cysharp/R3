namespace R3.Tests.OperatorTests;

public class DefaultIfEmptyTest
{
    [Fact]
    public void DefaultIfEmpty()
    {
        using var list = Observable.Empty<int>().DefaultIfEmpty().ToLiveList();

        list.AssertEqual([0]);
    }
}
