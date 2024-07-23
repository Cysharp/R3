namespace R3.Tests.OperatorTests;

public class MinMaxTest
{
    [Fact]
    public async Task Empty()
    {
        var o = Observable.Empty<int>();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await o.MinMaxAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await o.MinMaxAsync(x => x));
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).MinMaxAsync()).Should().Be((999, 999));
        (await Observable.Return(999).MinMaxAsync(x => 777)).Should().Be((777, 777));
    }

    [Fact]
    public async Task MultipleValues()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        (await source.MinMaxAsync()).Should().Be((1, 10));
        (await source.MinMaxAsync(x => x * 10)).Should().Be((10, 100));
    }

    [Fact]
    public async Task First()
    {
        var minMax = await new[] { 1, 5, 3, 4, 6, 7, 10 }.ToObservable().MinMaxAsync();
        minMax.Should().Be((1, 10));
    }

    [Fact]
    public async Task Last()
    {
        var minMax = await new[] { 10, 2, 3, 4, 6, 7, 1 }.ToObservable().MinMaxAsync();
        minMax.Should().Be((1, 10));
    }

    [Fact]
    public async Task Midway()
    {
        var minMax = await new[] { 2, 4, 10, 3, 1, 6, 7, 5 }.ToObservable().MinMaxAsync();
        minMax.Should().Be((1, 10));
    }

    [Fact]
    public async Task Error()
    {
        var o = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });
        await Assert.ThrowsAsync<Exception>(async () => await o.MinMaxAsync());
        await Assert.ThrowsAsync<Exception>(async () => await o.MinMaxAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await o.OnErrorResumeAsFailure().MinMaxAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await o.OnErrorResumeAsFailure().MinMaxAsync());
    }

    [Fact]
    public async Task SelectorError()
    {
        var o = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });
        await Assert.ThrowsAsync<Exception>(async () => await o.MinMaxAsync<int, int>(x => throw new Exception("bra")));
    }
}
