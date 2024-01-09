namespace R3.Tests.OperatorTests;

public class PrependTest
{
    [Fact]
    public void Prepend()
    {
        using var list = Observable.Range(1, 3).Prepend(0).ToLiveList();

        list.AssertEqual([0, 1, 2, 3]);
    }
}
