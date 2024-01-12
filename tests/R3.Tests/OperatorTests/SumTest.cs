using System.Numerics;

namespace R3.Tests.OperatorTests;

public class SumTest
{
    [Fact]
    public async Task Empty()
    {
        (await Observable.Empty<int>().SumAsync()).Should().Be(0);
        (await Observable.Empty<TestNumber>().SumAsync()).Value.Should().Be(0);
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).SumAsync()).Should().Be(999);
        (await Observable.Return(new TestNumber(999)).SumAsync()).Value.Should().Be(999);
    }

    [Fact]
    public async Task Error()
    {
        var error = Observable.Range(0, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.SumAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().SumAsync());
    }

    [Fact]
    public async Task MultipleValues()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        (await source.SumAsync()).Should().Be(36);

        var source2 = source.Select(x => new TestNumber(x));
        (await source2.SumAsync()).Value.Should().Be(36);
    }

    [Fact]
    public async Task WithSelector()
    {
        var source = new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.SumAsync(x => x * 10f)).Should().Be(360f);
        (await source.SumAsync(x => new TestNumber(x))).Value.Should().Be(36);
    }

    [Fact]
    public async Task WithSelectorError()
    {
        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.SumAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().SumAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await Observable.Range(0, 10).SumAsync(x => throw new Exception("bra")));
    }

    record struct TestNumber(int Value) : IAdditionOperators<TestNumber, TestNumber, TestNumber>
    {
        public static TestNumber operator +(TestNumber left, TestNumber right) =>
            new(left.Value + right.Value);
    }
}
