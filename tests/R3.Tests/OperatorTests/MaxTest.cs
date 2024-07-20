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
        var max = await new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable().MaxAsync();
        max.Should().Be(10);
    }

    [Fact]
    public async Task First()
    {
        var max = await new[] { 10, 1, 3, 4, 6, 7, 5 }.ToObservable().MaxAsync();
        max.Should().Be(10);
    }

    [Fact]
    public async Task Last()
    {
        var max = await new[] { 6, 2, 7, 1, 3, 4, 10 }.ToObservable().MaxAsync();
        max.Should().Be(10);
    }

    [Fact]
    public async Task Midway()
    {
        var max = await new[] { 6, 2, 7, 10, 3, 4, 1 }.ToObservable().MaxAsync();
        max.Should().Be(10);
    }

    [Fact]
    public async Task Empty()
    {
        var task = Observable.Empty<int>().MaxAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    [Fact]
    public async Task Error()
    {
        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.MaxAsync();
        });

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.OnErrorResumeAsFailure().MaxAsync();
        });
    }

    [Fact]
    public async Task WithComparer()
    {
        var result = await new[] { new TestData(100), new TestData(200) }.ToObservable().MaxAsync(new TestComparer());
        result.Value.Should().Be(200);
    }

    [Fact]
    public async Task WithSelector()
    {
        var source = new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.MaxAsync(x => x == 7 ? 777 : x)).Should().Be(777);
        // (await source.MaxAsync(x => new TestData(x), new TestComparer())).Value.Should().Be(1);
    }

    [Fact]
    public async Task WithSelectorError()
    {
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await Observable.Range(1, 10)
                .Select(x =>
                {
                    if (x == 3) throw new Exception("foo");
                    return x;
                })
                .MaxAsync(x => x);
        });

        var error = Observable.Range(1, 10)
            .Select(x =>
            {
                if (x == 3) throw new Exception("foo");
                return x;
            });

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.MaxAsync(x => x);
        });
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.OnErrorResumeAsFailure().MaxAsync(x => x);
        });
    }

    record struct TestData(int Value);

    class TestComparer : IComparer<TestData>
    {
        public int Compare(TestData x, TestData y) => x.Value.CompareTo(y.Value);
    }
}
