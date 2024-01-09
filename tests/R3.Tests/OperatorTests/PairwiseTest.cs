namespace R3.Tests.OperatorTests;

public class PairwiseTest
{
    [Fact]
    public void Pairwise()
    {
        using var list = Observable.Range(1, 3).Pairwise().ToLiveList();

        list.AssertEqual(
            [
                (1, 2),
                (2, 3)
            ]);
    }
}
