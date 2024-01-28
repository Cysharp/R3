namespace R3.Tests.OperatorTests;

public class ToArrayTest
{
    [Fact]
    public async Task Complete()
    {
        var publisher = new Subject<int>();

        var listTask = publisher.ToArrayAsync();

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
    public async Task ResultCompletableFault()
    {
        var publisher = new Subject<int>();

        var listTask = publisher.ToArrayAsync();

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        listTask.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.OnCompleted(Result.Failure(new Exception("foo")));

        await Assert.ThrowsAsync<Exception>(async () => await listTask);
    }

    [Fact]
    public async Task ResultCompletableCancel()
    {
        var cts = new CancellationTokenSource();
        var isDisposed = false;

        var publisher = new Subject<int>();

        var listTask = publisher.Do(onDispose: () => isDisposed = true).ToArrayAsync(cts.Token);

        publisher.OnNext(1);
        publisher.OnNext(2);
        publisher.OnNext(3);
        publisher.OnNext(4);
        publisher.OnNext(5);

        listTask.Status.Should().Be(TaskStatus.WaitingForActivation);

        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await listTask);

        isDisposed.Should().BeTrue();
    }
}
