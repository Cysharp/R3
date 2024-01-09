namespace R3.Tests.OperatorTests;

public class ScanTest
{
    [Fact]
    public void Scan()
    {
        using var list = Observable
            .Range(1, 3)
            .Scan(static (first, second) => first + second)
            .ToLiveList();

        list.AssertEqual([ 1, 3, 6 ]);
    }

    [Fact]
    public void ScanWithSeed()
    {
        using var list = Observable
            .Range(1, 3)
            .Scan(1, static (first, second) => first + second)
            .ToLiveList();

        list.AssertEqual([ 2, 4, 7]);
    }
}
