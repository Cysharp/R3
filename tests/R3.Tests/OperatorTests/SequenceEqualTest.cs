namespace R3.Tests.OperatorTests;

public class SequenceEqualTest
{
    [Fact]
    public async Task Positive()
    {
        var range1 = Observable.Range(1, 10);
        var range2 = Observable.Range(1, 10);

        var task = range1.SequenceEqualAsync(range2);
        (await task).Should().BeTrue();
    }

    [Fact]
    public async Task Negative()
    {
        var range1 = Observable.Range(1, 10);
        var range2 = Observable.Range(1, 11);

        var task = range1.SequenceEqualAsync(range2);
        (await task).Should().BeFalse();
    }
}
