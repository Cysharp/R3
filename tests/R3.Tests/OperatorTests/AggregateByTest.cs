namespace R3.Tests.OperatorTests;

public class AggregateByTest
{
    [Fact]
    public async Task AggregateBy()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateByAsync(x => x % 2 == 0, 100, (sum, x) => x + sum);

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        var result = await task;
        result.FirstOrDefault(x => x.Key).Value.Should().Be(106);
        result.FirstOrDefault(x => !x.Key).Value.Should().Be(109);
    }

    [Fact]
    public async Task AggregateBy_Empty()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateByAsync(x => x % 2 == 0, 100, (sum, x) => x + sum);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);
        publisher.OnCompleted();

        (await task).Should().BeEmpty();
    }

    [Fact]
    public async Task AggregateBy_One()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateByAsync(x => x % 2 == 0, 100, (sum, x) => x + sum);

        publisher.OnNext(2);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);
        publisher.OnCompleted();

        var result = await task;
        result.Count().Should().Be(1);
        result.First().Value.Should().Be(102);
    }

    [Fact]
    public async Task AggregateBy_SeedSelector()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateByAsync(
            x => x % 2 == 0,
            key => key ? 100 : 0,
            (sum, x) => x + sum);

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        var result = await task;
        result.FirstOrDefault(x => x.Key).Value.Should().Be(106);
        result.FirstOrDefault(x => !x.Key).Value.Should().Be(9);
    }
}
