namespace R3.Tests.OperatorTests;

public class ToLookupTest
{
    [Fact]
    public async Task ToLookup()
    {
        var publisher = new Subject<KeyValuePair<int, string>>();

        var task = publisher.ToLookupAsync(static x => x.Key);

        publisher.OnNext(new (1, "a"));
        publisher.OnNext(new (2, "b"));
        publisher.OnNext(new (3, "c"));

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        task.Status.Should().Be(TaskStatus.RanToCompletion);

        var expected = new Dictionary<int, string>
        {
            [1] = "a",
            [2] = "b",
            [3] = "c"
        }
        .ToLookup(static x => x.Key);

        (await task).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ToLookupWithElementSelector()
    {
        var publisher = new Subject<(int, string)>();

        var task = publisher.ToLookupAsync(static x => x.Item1, static x => x.Item2.ToUpperInvariant());

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
        }
        .ToLookup(static x => x.Key, static x => x.Value);

        (await task).Should().BeEquivalentTo(expected);
    }
}
