using System.Numerics;

namespace R3.Tests.OperatorTests;

public class SumTest
{
    [Fact]
    public async Task Empty()
    {
        (await Observable.Empty<int>().SumAsync()).Should().Be(0);
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).SumAsync()).Should().Be(999);
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
        var sum = await source.SumAsync();

        sum.Should().Be(36);
    }
    //
    // [Fact]
    // public async Task NumberType()
    // {
    //
    // }
    //
    // record struct TestNumber(int Value) : IAdditionOperators<TestNumber, TestNumber, TestNumber>
    // {
    //     public static TestNumber operator +(TestNumber left, TestNumber right) =>
    //         new(left.Value + right.Value);
    // }
}
