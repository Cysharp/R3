namespace R3.Tests.OperatorTests;

public class AppendTest
{
    [Fact]
    public void Append()
    {
        using var list = Observable.Range(1, 3).Append(4).ToLiveList();

        list.AssertEqual([1, 2, 3, 4]);
    }
}
