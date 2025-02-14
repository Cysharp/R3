namespace R3.Tests.OperatorTests;

public class CountTest
{
    [Fact]
    public async Task Empty()
    {
        (await Observable.Empty<int>().CountAsync()).ShouldBe(0);
        (await Observable.Empty<int>().CountAsync(_ => true)).ShouldBe(0);
        (await Observable.Empty<long>().LongCountAsync()).ShouldBe(0);
        (await Observable.Empty<long>().LongCountAsync(_ => true)).ShouldBe(0);
    }

    [Fact]
    public async Task MultipleValues()
    {
        var source = new [] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.CountAsync()).ShouldBe(8);
        (await source.Select(x => (long)x).CountAsync()).ShouldBe(8);

        (await source.CountAsync(x => x % 2 == 0)).ShouldBe(4);
        (await source.Select(x => (long)x).CountAsync(x => x % 2== 0)).ShouldBe(4);
    }

    [Fact]
    public async Task Filter()
    {
        var source = new [] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.CountAsync(x => x % 2 == 0)).ShouldBe(4);
        (await source.Select(x => (long)x).CountAsync(x => x % 2== 0)).ShouldBe(4);
    }

    [Fact]
    public async Task Error()
    {
        var error = Observable.Range(0, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.CountAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.LongCountAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().CountAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().LongCountAsync());
    }

    [Fact]
    public async Task PredicateError()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await source.CountAsync(_ => throw new Exception("hoge"));
        });
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await source.LongCountAsync(_ => throw new Exception("hoge"));
        });
    }
}
