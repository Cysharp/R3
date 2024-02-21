using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class SubscribeAwaitTest
{
    [Fact]
    public void Sequential()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.Sequential);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(3);
        liveList.Should().Equal([100, 200, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void Drop()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.Drop);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void Parallel()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.Parallel);

        subject.OnNext(1);
        subject.OnNext(2);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 200]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(2);
        liveList.Should().Equal([100, 200, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void Switch()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.Switch);

        subject.OnNext(1);
        subject.OnNext(2); // disposed 1

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([200]);

        timeProvider.Advance(2);
        liveList.Should().Equal([200]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.Should().Equal([200]);

        timeProvider.Advance(3);
        liveList.Should().Equal([200, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void ParallelLimit()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.Parallel, maxConcurrent: 2);

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1); // start 3
        liveList.Should().Equal([100, 200]);

        timeProvider.Advance(3);
        liveList.Should().Equal([100, 200, 300]);

        subject.OnCompleted();
    }

    [Fact]
    public void ThrottleFirstLast()
    {
        SynchronizationContext.SetSynchronizationContext(null); // xUnit insert fucking SynchronizationContext so ignore it.

        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        var liveList = new List<int>();
        using var _ = subject
            .SubscribeAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                liveList.Add(x * 100);
            }, AwaitOperation.ThrottleFirstLast);

        subject.OnNext(1);
        subject.OnNext(2);
        subject.OnNext(3);
        subject.OnNext(4);
        subject.OnNext(5);

        liveList.Should().Equal([]);

        timeProvider.Advance(2);
        liveList.Should().Equal([]);

        timeProvider.Advance(1);
        liveList.Should().Equal([100]);

        timeProvider.Advance(3);
        liveList.Should().Equal([100,500]);

        subject.OnNext(6);
        subject.OnNext(7);
        subject.OnNext(8);
        subject.OnNext(9);

        timeProvider.Advance(3);
        liveList.Should().Equal([100,500,600]);

        timeProvider.Advance(3);
        liveList.Should().Equal([100,500,600,900]);

        subject.OnCompleted();
    }
}
