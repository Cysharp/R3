using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace R3.Tests.OperatorTests;

public class SelectAwaitTest
{
    [Fact]
    public void Sequential()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Sequential, configureAwait: false)
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
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 200, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task SequentialCancel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        bool canceled = false;
        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                    return x * 100;
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    throw;
                }
            }, AwaitOperation.Sequential)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100]);

        canceled.Should().BeFalse();

        liveList.Dispose();

        await Task.Delay(TimeSpan.FromSeconds(1));

        canceled.Should().BeTrue();
    }

    [Fact]
    public void Drop()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Drop)
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

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task DropCancel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();
        bool canceled = false;

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                    return x * 100;
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    throw;
                }
            }, AwaitOperation.Drop)
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
        Thread.Sleep(100);
        liveList.AssertEqual([100]);

        subject.OnNext(3);

        canceled.Should().BeFalse();

        liveList.Dispose();

        await Task.Delay(TimeSpan.FromSeconds(1));

        canceled.Should().BeTrue();
    }

    [Fact]
    public void Parallel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Parallel)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 200]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 200, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task ParallelCancel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();
        var canceled = false;

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                    return x * 100;
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    throw;
                }
            }, AwaitOperation.Parallel)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 200]);

        subject.OnNext(3);

        canceled.Should().BeFalse();
        liveList.Dispose();

        await Task.Delay(TimeSpan.FromSeconds(1));

        canceled.Should().BeTrue();
    }


    [Fact]
    public void SequentialOnCompleted()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Sequential, configureAwait: false)
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
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 200, 300]);

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
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Switch, configureAwait: false)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2); // disposed 1

        liveList.AssertEqual([]);

        timeProvider.Advance(2);
        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([200]);

        timeProvider.Advance(2);
        liveList.AssertEqual([200]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.AssertEqual([200]);

        timeProvider.Advance(3);
        liveList.AssertEqual([200, 300]);

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
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(x), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.SequentialParallel, configureAwait: false)
            .ToLiveList();

        subject.OnNext(2); // 2 seconds wait
        subject.OnNext(1); // 1 seconds wait

        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([]); // 1 seconds complete but not yet complete

        timeProvider.Advance(1);
        liveList.AssertEqual([200, 100]); // both complete

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.AssertEqual([200, 100]);

        timeProvider.Advance(2);
        liveList.AssertEqual([200, 100, 300]);

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
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Parallel, maxConcurrent: 2)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3); // enqueue

        liveList.AssertEqual([]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 200, 300]);

        subject.OnNext(4);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 200, 300, 400]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void SequentialParallelLimit()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(x), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.SequentialParallel, configureAwait: false, cancelOnCompleted: false, maxConcurrent: 2)
            .ToLiveList();

        subject.OnNext(2); // 2 seconds wait
        subject.OnNext(1); // 1 seconds wait
        subject.OnNext(3); // 3 seconds enqueue
        subject.OnNext(2); // 1 seconds enqueue

        liveList.AssertEqual([]);

        timeProvider.Advance(1);
        liveList.AssertEqual([]); // 1 seconds complete but not yet complete, start 3 seconds

        timeProvider.Advance(1);
        liveList.AssertEqual([200, 100]); // both complete

        timeProvider.Advance(2);
        liveList.AssertEqual([200, 100, 300, 200]);

        subject.OnNext(2); // 2 seconds wait
        subject.OnNext(1); // 1 seconds wait
        subject.OnNext(3); // 3 seconds enqueue
        subject.OnNext(2); // 1 seconds enqueue

        subject.OnCompleted();

        timeProvider.Advance(2);
        liveList.AssertEqual([200, 100, 300, 200, 200, 100]);
        timeProvider.Advance(3);
        liveList.AssertEqual([200, 100, 300, 200, 200, 100, 300, 200]);

        liveList.AssertIsCompleted();
    }

    [Fact]
    public void ThrottleFirstLast()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.ThrottleFirstLast, configureAwait: false)
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
        liveList.AssertEqual([100, 500]);

        subject.OnNext(6);
        subject.OnNext(7);
        subject.OnNext(8);
        subject.OnNext(9);

        timeProvider.Advance(1);
        liveList.AssertEqual([100, 500]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100, 500, 600]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100, 500, 600, 900]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task ThrottleFirstLastCancel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        bool canceled = false;
        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                    return x * 100;
                }
                catch (OperationCanceledException)
                {
                    canceled = true;
                    throw;
                }
            }, AwaitOperation.ThrottleFirstLast)
            .ToLiveList();

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.AssertEqual([]);

        timeProvider.Advance(3);
        liveList.AssertEqual([100]);

        canceled.Should().BeFalse();

        liveList.Dispose();

        await Task.Delay(TimeSpan.FromSeconds(1));

        canceled.Should().BeTrue();
    }
}
