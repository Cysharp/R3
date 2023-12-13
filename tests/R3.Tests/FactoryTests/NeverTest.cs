namespace R3.Tests.FactoryTests;

public class NeverTest
{
    [Fact]
    public void Never()
    {
        using var list = Event.Never<int>().ToLiveList();
        list.AssertEqual([]);
    }

    // NeverComplete test
    [Fact]
    public void NeverComplete()
    {
        using var list = Event.NeverComplete<int, int>().ToLiveList();
        list.AssertIsNotCompleted();
    }
}
