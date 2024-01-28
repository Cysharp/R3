namespace R3.Tests.OperatorTests;

public class AggregateTest
{
    [Fact]
    public async Task Reduce()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateAsync((result, x) => result + x);

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        (await task).Should().Be(15);
    }

    [Fact]
    public async Task ReduceEmptyElement()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateAsync((result, x) => result + x);
        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);
    }

    [Fact]
    public async Task ReduceOneElement()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateAsync((result, x) => result + x);
        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnNext(100);
        publisher.OnCompleted();

        (await task).Should().Be(100);
    }

    [Fact]
    public async Task Aggregate()
    {
        var publisher = new Subject<int>();

        var listTask = publisher.AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; });

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        listTask.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        (await listTask).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task AggregateWithResultSelector()
    {
        var publisher = new Subject<int>();

        var task = publisher.AggregateAsync(0, (max, x) => max < x ? x : max, x => x.ToString());

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        task.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted();

        (await task).Should().Be("5");
    }

    [Fact]
    public async Task ImmediateCompleted()
    {
        var range = Observable.Range(1, 5);
        var listTask = range.AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x) => x);
        (await listTask).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task BeforeCanceled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var publisher = new Subject<int>();
        var isDisposed = false;

        var listTask = publisher
            .Do(onDispose: () => isDisposed = true)
            .AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x) => x, cts.Token);


        isDisposed.Should().BeTrue();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await listTask);
    }
}
