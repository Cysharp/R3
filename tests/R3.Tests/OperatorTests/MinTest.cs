namespace R3.Tests.OperatorTests;

public class MinTest
{
    [Fact]
    public async Task Empty()
    {
        var task = Observable.Empty<int>().MinAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).MinAsync()).Should().Be(999);
    }

    [Fact]
    public async Task MultipleValue()
    {
        var min = await new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable().MinAsync();
        min.Should().Be(1);
    }

    [Fact]
    public async Task First()
    {
        var min = await new[] { 1, 10, 3, 4, 6, 7, 5 }.ToObservable().MinAsync();
        min.Should().Be(1);
    }

    [Fact]
    public async Task Last()
    {
        var min = await new[] { 2, 10, 3, 4, 6, 7, 1 }.ToObservable().MinAsync();
        min.Should().Be(1);
    }

    [Fact]
    public async Task Midway()
    {
        var min = await new[] { 2, 10, 3, 4, 1, 6, 7, 5 }.ToObservable().MinAsync();
        min.Should().Be(1);
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
            await error.MinAsync();
        });

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.OnErrorResumeAsFailure().MinAsync();
        });
    }

    [Fact]
    public async Task WithComparer()
    {
        var result = await new[] { new TestData(100), new TestData(200) }.ToObservable().MinAsync(new TestComparer());
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task WithSelector()
    {
        var source = new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.MinAsync(x => x == 7 ? -1 : x)).Should().Be(-1);
        // (await source.MinAsync(x => new TestData(x), new TestComparer())).Value.Should().Be(10);
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
                .MinAsync(x => x);
        });

        var error = Observable.Range(1, 10)
            .Select(x =>
            {
                if (x == 3) throw new Exception("foo");
                return x;
            });

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.MinAsync(x => x);
        });
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await error.OnErrorResumeAsFailure().MinAsync(x => x);
        });
    }

    record struct TestData(int Value);

    class TestComparer : IComparer<TestData>
    {
        public int Compare(TestData x, TestData y) => x.Value.CompareTo(y.Value);
    }
}
