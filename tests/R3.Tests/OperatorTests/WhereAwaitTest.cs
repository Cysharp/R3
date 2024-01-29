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
}
