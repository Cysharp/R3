namespace R3.Tests.OperatorTests;

public class ToAsyncEnumerableTest
{
    [Fact]
    async void Test()
    {
        var publisher = new Subject<int>();
        var cts = new CancellationTokenSource();
        var e = publisher.ToAsyncEnumerable(cts.Token);

        publisher.OnNext(1);
        publisher.OnNext(10);
        publisher.OnNext(100);
        publisher.OnCompleted();

        var l = new List<int>();
        await foreach (var item in e)
        {
            l.Add(item);
        }

        l.Should().Equal([1, 10, 100]);
    }

    [Fact]
    async void Cancel()
    {
        var publisher = new Subject<int>();
        var cts = new CancellationTokenSource();

        var disposed = false;
        var e = publisher.Do(onDispose: () => disposed = true).ToAsyncEnumerable(cts.Token);

        publisher.OnNext(1);
        publisher.OnNext(10);

        publisher.OnNext(100);
        // publisher.OnCompleted();

        var l = new List<int>();

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await foreach (var item in e)
            {
                l.Add(item);
                if (item == 10)
                {
                    cts.Cancel();
                }
            }
        });

        l.Should().Equal([1, 10, 100]);
        disposed.Should().BeTrue();
    }
}
