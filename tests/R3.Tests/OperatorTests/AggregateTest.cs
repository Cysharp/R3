using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class AggregateTest
{
    [Fact]
    public async Task Aggreagte()
    {
        var publisher = new CompletablePublisher<int, Unit>();

        var listTask = publisher.AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x, _) => x);

        publisher.PublishOnNext(1);
        publisher.PublishOnNext(2);
        publisher.PublishOnNext(3);
        publisher.PublishOnNext(4);
        publisher.PublishOnNext(5);

        listTask.Status.Should().Be(TaskStatus.WaitingForActivation);

        publisher.PublishOnCompleted(Unit.Default);

        (await listTask).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task ImmediateCompleted()
    {
        var range = EventFactory.Range(1, 5);
        var listTask = range.AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x, _) => x);
        (await listTask).Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public async Task BeforeCanceled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var publisher = new CompletablePublisher<int, Unit>();
        var isDisposed = false;

        var listTask = publisher
            .DoOnDisposed(() => isDisposed = true)
            .AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x, _) => x, cts.Token);


        isDisposed.Should().BeTrue();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await listTask);
    }

    // and Aggregate used operators

    [Fact]
    public async Task ToHashSet()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var set = await source.ToHashSetAsync();

        set.Should().BeEquivalentTo([1, 10, 3, 4, 6, 7]);
    }

    [Fact]
    public async Task Count()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var count = await source.CountAsync();

        count.Should().Be(8);

        var count2 = await EventFactory.Empty<int>().CountAsync();
        count2.Should().Be(0);
    }

    [Fact]
    public async Task LongCount()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var count = await source.LongCountAsync();

        count.Should().Be(8);

        var count2 = await EventFactory.Empty<int>().LongCountAsync();
        count2.Should().Be(0);

        var error = EventFactory.Throw<int>(new Exception("foo"));

        await Assert.ThrowsAsync<Exception>(async () => await error.LongCountAsync());
    }

    [Fact]
    public async Task Min()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var min = await source.MinAsync();

        min.Should().Be(1);

        (await EventFactory.Return(999).MinAsync()).Should().Be(999);

        var task = EventFactory.Empty<int>().MinAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = EventFactory.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorAsComplete();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinAsync());
    }

    [Fact]
    public async Task Max()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var min = await source.MaxAsync();

        min.Should().Be(10);

        (await EventFactory.Return(999).MaxAsync()).Should().Be(999);

        var task = EventFactory.Empty<int>().MaxAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = EventFactory.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorAsComplete();
        await Assert.ThrowsAsync<Exception>(async () => await error.MaxAsync());
    }

    [Fact]
    public async Task MinMax()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var minmax = await source.MinMaxAsync();

        minmax.Min.Should().Be(1);
        minmax.Max.Should().Be(10);

        var mm2 = await EventFactory.Return(999).MinMaxAsync();
        mm2.Min.Should().Be(999);
        mm2.Max.Should().Be(999);

        var task = EventFactory.Empty<int>().MaxAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = EventFactory.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorAsComplete();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinMaxAsync());
    }

    [Fact]
    public async Task Sum()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var sum = await source.SumAsync();

        sum.Should().Be(36);

        (await EventFactory.Return(999).SumAsync()).Should().Be(999);

        var task = EventFactory.Empty<int>().SumAsync();
        (await task).Should().Be(0);

        var error = EventFactory.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorAsComplete();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinAsync());
    }

    [Fact]
    public async Task Avg()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        var avg = await source.AverageAsync();

        avg.Should().Be(new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.Average());

        (await EventFactory.Return(999).AverageAsync()).Should().Be(999);

        var task = EventFactory.Empty<int>().AverageAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = EventFactory.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorAsComplete();
        await Assert.ThrowsAsync<Exception>(async () => await error.AverageAsync());
    }

    [Fact]
    public async Task WaitAsync()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToEvent();
        await source.WaitAsync();

        var p = new CompletablePublisher<int, string>();
        var task = p.WaitAsync();

        p.PublishOnNext(10);
        p.PublishOnNext(20);
        p.PublishOnNext(30);
        p.PublishOnCompleted("foo");

        (await task).Should().Be("foo");
    }
}
