namespace R3.Tests.OperatorTests;

public class MaxTest
{
    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).MaxAsync()).Should().Be(999);
    }

    [Fact]
    public async Task MultipleValue()
    {
        var min = await new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable().MaxAsync();
        min.Should().Be(10);
    }

    [Fact]
    public async Task Empty()
    {
        var task = Observable.Empty<int>().MaxAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    [Fact]
    public async Task WithError()
    {
        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.MaxAsync());
    }

    [Fact]
    public async Task WithComparer()
    {
        var result = await new[] { new TestData(100), new TestData(200) }.ToObservable().MaxAsync(new TestComparer());
        result.Value.Should().Be(200);
    }

    record struct TestData(int Value);

    class TestComparer : IComparer<TestData>
    {
        public int Compare(TestData x, TestData y) => x.Value.CompareTo(y.Value);
    }
}
