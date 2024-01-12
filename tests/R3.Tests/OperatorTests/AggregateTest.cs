using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class AggregateTest
{
    [Fact]
    public async Task Aggreagte()
    {
        var publisher = new Subject<int>();

        var listTask = publisher.AggregateAsync(new List<int>(), (x, i) => { x.Add(i); return x; }, (x) => x);

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

    // and Aggregate used operators

    [Fact]
    public async Task ToHashSet()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var set = await source.ToHashSetAsync();

        set.Should().BeEquivalentTo([1, 10, 3, 4, 6, 7]);
    }

    [Fact]
    public async Task Min()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var min = await source.MinAsync();

        min.Should().Be(1);

        (await Observable.Return(999).MinAsync()).Should().Be(999);

        var task = Observable.Empty<int>().MinAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinAsync());
    }

    [Fact]
    public async Task Max()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var min = await source.MaxAsync();

        min.Should().Be(10);

        (await Observable.Return(999).MaxAsync()).Should().Be(999);

        var task = Observable.Empty<int>().MaxAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.MaxAsync());
    }

    [Fact]
    public async Task MinMax()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var minmax = await source.MinMaxAsync();

        minmax.Min.Should().Be(1);
        minmax.Max.Should().Be(10);

        var mm2 = await Observable.Return(999).MinMaxAsync();
        mm2.Min.Should().Be(999);
        mm2.Max.Should().Be(999);

        var task = Observable.Empty<int>().MaxAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinMaxAsync());
    }

    [Fact]
    public async Task Sum()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var sum = await source.SumAsync();

        sum.Should().Be(36);

        (await Observable.Return(999).SumAsync()).Should().Be(999);

        var task = Observable.Empty<int>().SumAsync();
        (await task).Should().Be(0);

        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.MinAsync());
    }

    [Fact]
    public async Task Avg()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        var avg = await source.AverageAsync();

        avg.Should().Be(new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.Average());

        (await Observable.Return(999).AverageAsync()).Should().Be(999);

        var task = Observable.Empty<int>().AverageAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        }).OnErrorResumeAsFailure();
        await Assert.ThrowsAsync<Exception>(async () => await error.AverageAsync());
    }

    [Fact]
    public async Task WaitAsync()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        await source.WaitAsync();

        var p = new Subject<int>();
        var task = p.WaitAsync();

        p.OnNext(10);
        p.OnNext(20);
        p.OnNext(30);
        p.OnCompleted();

        await task;
    }
}
