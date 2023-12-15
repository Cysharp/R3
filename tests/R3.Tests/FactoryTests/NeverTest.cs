namespace R3.Tests.FactoryTests;

public class NeverTest
{
    [Fact]
    public void Never()
    {
        using var list = Observable.Never<int>().ToLiveList();
        list.AssertEqual([]);
    }
}
