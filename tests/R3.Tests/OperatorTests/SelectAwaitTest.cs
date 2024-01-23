using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

// TODO: OnCompleted test

public class SelectAwaitTest
{
    [Fact]
    public void Sequential()
    {
        var subject = new Subject<int>();
        var timeProvider = new FakeTimeProvider();

        using var liveList = subject
            .SelectAwait(async (x, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), timeProvider, ct);
                return x * 100;
            }, AwaitOperation.Sequential)
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

        timeProvider.Advance(1);
        Thread.Sleep(100);
        liveList.AssertEqual([100, 200]);

        timeProvider.Advance(3);
        Thread.Sleep(100);
        liveList.AssertEqual([100, 200, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task SequentialCancel()
    {
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
        Thread.Sleep(100);
        liveList.AssertEqual([100]);

        timeProvider.Advance(2);
        liveList.AssertEqual([100]);

        subject.OnNext(3);

        timeProvider.Advance(1);
        liveList.AssertEqual([100]);

        timeProvider.Advance(2);
        Thread.Sleep(100);
        liveList.AssertEqual([100, 300]);

        subject.OnCompleted();

        liveList.AssertIsCompleted();
    }

    [Fact]
    public async Task DropCancel()
    {
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


}
