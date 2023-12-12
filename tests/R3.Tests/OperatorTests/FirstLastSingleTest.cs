using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class FirstLastSingleTest
{
    // Empty, 1, ...

    // Event First
    // Completable First/FirstOrDefault/Last/LastOrDefault

    [Fact]
    public async Task First()
    {
        var publisher = new Publisher<int>();
        var task = publisher.FirstAsync();
        publisher.PublishOnNext(10);
        (await task).Should().Be(10);

        var task2 = publisher.FirstAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);

        (await task2).Should().Be(15);

        var cts = new CancellationTokenSource();
        var task3 = publisher.FirstAsync(cts.Token);
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task3);

        var task4 = publisher.FirstAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        (await task4).Should().Be(99);
    }

    // CompletablePublisher First Test
    [Fact]
    public async Task First2()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.FirstAsync();
        publisher.PublishOnNext(10);
        (await task).Should().Be(10);

        var task2 = publisher.FirstAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);

        (await task2).Should().Be(15);

        var task3 = publisher.FirstAsync();

        publisher.PublishOnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.FirstAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task FirstOrDefault()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.FirstOrDefaultAsync();
        publisher.PublishOnNext(10);
        (await task).Should().Be(10);

        var task2 = publisher.FirstOrDefaultAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);

        (await task2).Should().Be(15);

        var task3 = publisher.FirstOrDefaultAsync(9999);

        publisher.PublishOnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.FirstOrDefaultAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task LastAsync()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.LastAsync();
        publisher.PublishOnNext(10);
        publisher.PublishOnCompleted(default);
        (await task).Should().Be(10);

        publisher = new CompletablePublisher<int, Unit>();
        var task2 = publisher.LastAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);
        publisher.PublishOnCompleted(default);

        (await task2).Should().Be(25);

        publisher = new CompletablePublisher<int, Unit>();
        var task3 = publisher.LastAsync();

        publisher.PublishOnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.LastAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        publisher.PublishOnNext(11);
        publisher.PublishOnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task LastOrDefaultAsync()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.LastOrDefaultAsync();
        publisher.PublishOnNext(10);
        publisher.PublishOnCompleted(default);
        (await task).Should().Be(10);

        publisher = new CompletablePublisher<int, Unit>();
        var task2 = publisher.LastOrDefaultAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);
        publisher.PublishOnCompleted(default);

        (await task2).Should().Be(25);

        publisher = new CompletablePublisher<int, Unit>();
        var task3 = publisher.LastOrDefaultAsync(9999);

        publisher.PublishOnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.LastOrDefaultAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        publisher.PublishOnNext(11);
        publisher.PublishOnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task Single()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.SingleAsync();
        publisher.PublishOnNext(10);
        publisher.PublishOnCompleted();
        (await task).Should().Be(10);

        publisher = new CompletablePublisher<int, Unit>();
        var task2 = publisher.SingleAsync();
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task2);

        publisher = new CompletablePublisher<int, Unit>();
        var task3 = publisher.SingleAsync();

        publisher.PublishOnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.SingleAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        publisher.PublishOnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task SingleOrDefault()
    {
        var publisher = new CompletablePublisher<int, Unit>();
        var task = publisher.SingleOrDefaultAsync(9999);
        publisher.PublishOnNext(10);
        publisher.PublishOnCompleted();
        (await task).Should().Be(10);

        publisher = new CompletablePublisher<int, Unit>();
        var task2 = publisher.SingleOrDefaultAsync(9999);
        publisher.PublishOnNext(15);
        publisher.PublishOnNext(25);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task2);

        publisher = new CompletablePublisher<int, Unit>();
        var task3 = publisher.SingleOrDefaultAsync(9999);

        publisher.PublishOnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new CompletablePublisher<int, Unit>();
        var task4 = publisher.SingleOrDefaultAsync(x => x % 3 == 0);
        publisher.PublishOnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.PublishOnNext(99);
        publisher.PublishOnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task ErrorStream()
    {
        var publisher = new CompletablePublisher<int, Result<Unit>>();
        var task = publisher.LastAsync();

        publisher.PublishOnNext(10);
        publisher.PublishOnNext(20);
        publisher.PublishOnNext(30);
        publisher.PublishOnCompleted(Result.Failure(new Exception("foo")));

        await Assert.ThrowsAsync<Exception>(async () => await task);
    }
}
