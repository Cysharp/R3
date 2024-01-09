namespace R3.Tests.OperatorTests;

public class DistinctUntilChangedTest
{
    [Fact]
    public void DistinctUntilChanged()
    {
        var source = new Subject<int>();

        using var list = source.DistinctUntilChanged().ToLiveList();

        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        source.OnNext(3);
        source.OnNext(2);
        source.OnNext(1);

        list.AssertEqual([1, 2, 3, 2, 1]);
    }
}
