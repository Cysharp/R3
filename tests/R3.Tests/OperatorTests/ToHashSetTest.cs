namespace R3.Tests.OperatorTests;

public class ToHashSetTest
{
    [Fact]
    public async Task Many()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var set = await source.ToHashSetAsync();

        set.Should().BeEquivalentTo([1, 10, 3, 4, 6, 7]);
    }

    [Fact]
    public async Task SetComparer()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var set = await source.ToHashSetAsync(new TestComparer());

        set.Should().BeEquivalentTo([1, 10, 3, 4, 6, 7]);
        set.Comparer.GetType().Should().Be(typeof(TestComparer));
    }

    class TestComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y) => x == y;
        public int GetHashCode(int obj) => obj.GetHashCode();
    }
}
