namespace R3.Tests.OperatorTests;

public class DistinctTest
{
    [Fact]
    public void Distinct()
    {
        var source = new Subject<int>();

        using var list = source.Distinct().ToLiveList();

        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        source.OnNext(4);

        list.AssertEqual([1, 2, 3, 4]);
    }
}
