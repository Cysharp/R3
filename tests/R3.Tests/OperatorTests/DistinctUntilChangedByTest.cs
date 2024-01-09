namespace R3.Tests.OperatorTests;

public class DistinctUntilChangedByTest
{
    [Fact]
    public void DistinctUntilChangedBy()
    {
        var source = new Subject<(int, int)>();

        using var list = source.DistinctUntilChangedBy(static x => x.Item1).ToLiveList();

        source.OnNext((1, 10));
        source.OnNext((2, 20));
        source.OnNext((3, 30));
        source.OnNext((3, 300));
        source.OnNext((2, 200));
        source.OnNext((1, 100));

        list.AssertEqual(
            [
                (1, 10),
                (2, 20),
                (3, 30),
                (2, 200),
                (1, 100)
            ]);
    }
}
