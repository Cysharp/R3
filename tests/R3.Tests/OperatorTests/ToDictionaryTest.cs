namespace R3.Tests.OperatorTests;

public class ToDictionaryTest
{
    [Fact]
    public async Task ToDictionary()
    {
        var publisher = new Subject<(int, string)>();

        var task = publisher.ToDictionaryAsync(static x => x.Item1);

        publisher.OnNext((1, "a"));
        publisher.OnNext((2, "b"));
        publisher.OnNext((3, "c"));

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        task.Status.Should().Be(TaskStatus.RanToCompletion);

        var expected = new Dictionary<int, (int, string)>
        {
            [1] = (1, "a"),
            [2] = (2, "b"),
            [3] = (3, "c")
        };

        (await task).Should().Equal(expected);
    }

    [Fact]
    public async Task ToDictionaryWithElementSelector()
    {
        var publisher = new Subject<(int, string)>();

        var task = publisher.ToDictionaryAsync(static x => x.Item1, static x => x.Item2.ToUpperInvariant());

        publisher.OnNext((1, "a"));
        publisher.OnNext((2, "b"));
        publisher.OnNext((3, "c"));

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        task.Status.Should().Be(TaskStatus.RanToCompletion);

        var expected = new Dictionary<int, string>
        {
            [1] = "A",
            [2] = "B",
            [3] = "C"
        };

        (await task).Should().Equal(expected);
    }
}
