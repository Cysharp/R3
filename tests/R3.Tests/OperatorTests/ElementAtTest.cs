namespace R3.Tests.OperatorTests;

public class ElementAtTest
{
    [Fact]
    public async Task ElementAt()
    {
        var xs = Observable.Range(0, 10);
        (await xs.ElementAtAsync(0)).ShouldBe(0);
        (await xs.ElementAtAsync(5)).ShouldBe(5);
        (await xs.ElementAtAsync(9)).ShouldBe(9);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtAsync(-1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtAsync(10));

        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtAsync(0));

        // empty case
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Observable.Empty<int>().ElementAtAsync(0));
    }

    [Fact]
    public async Task ElementAtOrDefault()
    {
        var xs = Observable.Range(0, 10);
        (await xs.ElementAtOrDefaultAsync(0)).ShouldBe(0);
        (await xs.ElementAtOrDefaultAsync(5)).ShouldBe(5);
        (await xs.ElementAtOrDefaultAsync(9)).ShouldBe(9);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtOrDefaultAsync(-1));
        (await xs.ElementAtOrDefaultAsync(10, 9999)).ShouldBe(9999);

        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtOrDefaultAsync(0));

        // empty case
        (await Observable.Empty<int>().ElementAtOrDefaultAsync(0, 99999)).ShouldBe(99999);
    }

    [Fact]
    public async Task ElementAtIndex()
    {
        var xs = Observable.Range(0, 10);
        (await xs.ElementAtAsync(new Index(0))).ShouldBe(0);
        (await xs.ElementAtAsync(new Index(5))).ShouldBe(5);
        (await xs.ElementAtAsync(new Index(9))).ShouldBe(9);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtAsync(new Index(10)));

        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtAsync(new Index(0)));

        // empty case
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Observable.Empty<int>().ElementAtAsync(new Index(0)));

        // more
        (await xs.ElementAtAsync(^1)).ShouldBe(9);
        (await xs.ElementAtAsync(^4)).ShouldBe(6);
        (await xs.ElementAtAsync(^10)).ShouldBe(0);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtAsync(^0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await xs.ElementAtAsync(^11));
        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtAsync(^1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await Observable.Empty<int>().ElementAtAsync(^1));
    }

    [Fact]
    public async Task ElementAtOrDefaultIndex()
    {
        var xs = Observable.Range(0, 10);
        (await xs.ElementAtOrDefaultAsync(new Index(0))).ShouldBe(0);
        (await xs.ElementAtOrDefaultAsync(new Index(5))).ShouldBe(5);
        (await xs.ElementAtOrDefaultAsync(new Index(9))).ShouldBe(9);

        (await xs.ElementAtOrDefaultAsync(new Index(10))).ShouldBe(0);

        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtOrDefaultAsync(new Index(0)));

        // empty case
        (await Observable.Empty<int>().ElementAtOrDefaultAsync(new Index(0))).ShouldBe(0);

        // more
        (await xs.ElementAtOrDefaultAsync(^1)).ShouldBe(9);
        (await xs.ElementAtOrDefaultAsync(^4)).ShouldBe(6);
        (await xs.ElementAtOrDefaultAsync(^10)).ShouldBe(0);

        (await xs.ElementAtOrDefaultAsync(^0, 9999)).ShouldBe(9999);
        (await xs.ElementAtOrDefaultAsync(^11, 9999)).ShouldBe(9999);
        await Assert.ThrowsAsync<Exception>(async () => await Observable.Throw<int>(new Exception()).ElementAtOrDefaultAsync(^1));
        (await Observable.Empty<int>().ElementAtOrDefaultAsync(^1, 9999)).ShouldBe(9999);
    }
}
