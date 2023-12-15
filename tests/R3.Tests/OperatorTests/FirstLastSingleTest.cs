using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class FirstLastSingleTest
{
    // Empty, 1, ...

    // Event First
    // Completable First/FirstOrDefault/Last/LastOrDefault

    // CompletablePublisher First Test
    [Fact]
    public async Task First2()
    {
        var publisher = new Subject<int>();
        var task = publisher.FirstAsync();
        publisher.OnNext(10);
        (await task).Should().Be(10);

        var task2 = publisher.FirstAsync();
        publisher.OnNext(15);
        publisher.OnNext(25);

        (await task2).Should().Be(15);

        var task3 = publisher.FirstAsync();

        publisher.OnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new Subject<int>();
        var task4 = publisher.FirstAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task FirstOrDefault()
    {
        var publisher = new Subject<int>();
        var task = publisher.FirstOrDefaultAsync();
        publisher.OnNext(10);
        (await task).Should().Be(10);

        var task2 = publisher.FirstOrDefaultAsync();
        publisher.OnNext(15);
        publisher.OnNext(25);

        (await task2).Should().Be(15);

        var task3 = publisher.FirstOrDefaultAsync(9999);

        publisher.OnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new Subject<int>();
        var task4 = publisher.FirstOrDefaultAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task LastAsync()
    {
        var publisher = new Subject<int>();
        var task = publisher.LastAsync();
        publisher.OnNext(10);
        publisher.OnCompleted(default);
        (await task).Should().Be(10);

        publisher = new Subject<int>();
        var task2 = publisher.LastAsync();
        publisher.OnNext(15);
        publisher.OnNext(25);
        publisher.OnCompleted(default);

        (await task2).Should().Be(25);

        publisher = new Subject<int>();
        var task3 = publisher.LastAsync();

        publisher.OnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new Subject<int>();
        var task4 = publisher.LastAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        publisher.OnNext(11);
        publisher.OnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task LastOrDefaultAsync()
    {
        var publisher = new Subject<int>();
        var task = publisher.LastOrDefaultAsync();
        publisher.OnNext(10);
        publisher.OnCompleted(default);
        (await task).Should().Be(10);

        publisher = new Subject<int>();
        var task2 = publisher.LastOrDefaultAsync();
        publisher.OnNext(15);
        publisher.OnNext(25);
        publisher.OnCompleted(default);

        (await task2).Should().Be(25);

        publisher = new Subject<int>();
        var task3 = publisher.LastOrDefaultAsync(9999);

        publisher.OnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new Subject<int>();
        var task4 = publisher.LastOrDefaultAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        publisher.OnNext(11);
        publisher.OnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task Single()
    {
        var publisher = new Subject<int>();
        var task = publisher.SingleAsync();
        publisher.OnNext(10);
        publisher.OnCompleted();
        (await task).Should().Be(10);

        publisher = new Subject<int>();
        var task2 = publisher.SingleAsync();
        publisher.OnNext(15);
        publisher.OnNext(25);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task2);

        publisher = new Subject<int>();
        var task3 = publisher.SingleAsync();

        publisher.OnCompleted(default);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task3);

        publisher = new Subject<int>();
        var task4 = publisher.SingleAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        publisher.OnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task SingleOrDefault()
    {
        var publisher = new Subject<int>();
        var task = publisher.SingleOrDefaultAsync(9999);
        publisher.OnNext(10);
        publisher.OnCompleted();
        (await task).Should().Be(10);

        publisher = new Subject<int>();
        var task2 = publisher.SingleOrDefaultAsync(9999);
        publisher.OnNext(15);
        publisher.OnNext(25);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task2);

        publisher = new Subject<int>();
        var task3 = publisher.SingleOrDefaultAsync(9999);

        publisher.OnCompleted(default);
        (await task3).Should().Be(9999);

        publisher = new Subject<int>();
        var task4 = publisher.SingleOrDefaultAsync(x => x % 3 == 0);
        publisher.OnNext(5);
        task4.Status.Should().NotBe(TaskStatus.RanToCompletion);

        publisher.OnNext(99);
        publisher.OnCompleted();
        (await task4).Should().Be(99);
    }

    [Fact]
    public async Task ErrorStream()
    {
        var publisher = new Subject<int>();
        var task = publisher.LastAsync();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);
        publisher.OnCompleted(Result.Failure(new Exception("foo")));

        await Assert.ThrowsAsync<Exception>(async () => await task);
    }
}
