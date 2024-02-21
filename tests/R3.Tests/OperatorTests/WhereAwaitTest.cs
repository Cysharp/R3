using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class WhereAwaitTest
{
    [Fact]
    public void Sequential()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.Sequential)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.AssertEqual([100]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void Drop()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.Drop)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        Thread.Sleep(100);
        liveList.AssertEqual([100]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100]);

        subject.OnNext(3);
        subject.OnNext(4);
        subject.OnNext(5);

        timeProvider.Advance(1);
        liveList.AssertEqual([100]);

        timeProvider.Advance(2);
        Thread.Sleep(100);
        liveList.AssertEqual([100, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void Parallel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.Parallel)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 300]);

        subject.OnNext(4);
        subject.OnNext(5);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 300, 500]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void Switch()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.Switch)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3); // 1, 2 is canceled.

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([300]);

        timeProvider.Advance(2);
        liveList.AssertEqual([300]);

        subject.OnNext(5);

        timeProvider.Advance(1);
        liveList.AssertEqual([300]);

        timeProvider.Advance(3);
        liveList.AssertEqual([300, 500]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void SequentialParallel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(x), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.SequentialParallel)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(2); // 2 seconds wait
        subject.OnNext(1); // 1 seconds wait
        subject.OnNext(3); // 3 seconds wait
        subject.OnNext(7); // 7 seconds wait
        subject.OnNext(5); // 5 seconds wait

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(2);

        liveList.AssertEqual([100, 300, 700, 500]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void ParallelLimit()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.Parallel, cancelOnCompleted: false, maxConcurrent: 2)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);

        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 300]);


        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);

        subject.OnCompleted();

        timeProvider.Advance(1);
        timeProvider.Advance(2);
        timeProvider.Advance(3);
        liveList.AssertEqual([100, 300, 100, 300]);

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void SequentialParallelLimit()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(x), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.SequentialParallel, maxConcurrent: 3)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(2); // 2 seconds wait
        subject.OnNext(1); // 1 seconds wait
        subject.OnNext(3); // 3 seconds wait
        subject.OnNext(7); // 7 seconds wait
        subject.OnNext(5); // 5 seconds wait

        liveList.AssertEqual([]);

        timeProvider.Advance(2); // deq 3, 7
        liveList.AssertEqual([100]);

        timeProvider.Advance(1); // deq 5
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 300]);

        timeProvider.Advance(4);
        liveList.AssertEqual([100, 300, 700, 500]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void ThrottleFirstLast()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .WhereAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x % 2 != 0;
            }, AwaitOperation.ThrottleFirstLast)
            .Select(x => x * 100)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnNext(4);
        subject.OnNext(5);
        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100,500]);

        subject.OnNext(6);
        subject.OnNext(7);
        subject.OnNext(8);
        subject.OnNext(9);
        subject.OnNext(10);
        subject.OnNext(11);

        timeProvider.Advance(3);
        liveList.AssertEqual([100,500]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100,500,1100]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

}
