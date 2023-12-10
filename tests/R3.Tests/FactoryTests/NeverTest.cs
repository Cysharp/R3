namespace R3.Tests.FactoryTests;

public class NeverTest
{
    [Fact]
    public void Never()
    {
        using var list = EventFactory.Never<int>().LiveRecord();
        list.AssertIsNotCompleted();
    }

    // NeverComplete test
    [Fact]
    public void NeverComplete()
    {
        using var list = EventFactory.NeverComplete<int, int>().LiveRecord();
        list.AssertIsNotCompleted();
    }
}
