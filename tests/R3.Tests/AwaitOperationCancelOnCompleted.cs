using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace R3.Tests;

public class AwaitOperationCancelOnCompleted
{
    [Theory]
    [InlineData(AwaitOperation.Sequential)]
    [InlineData(AwaitOperation.Drop)]
    [InlineData(AwaitOperation.Switch)]
    [InlineData(AwaitOperation.Parallel)]
    [InlineData(AwaitOperation.SequentialParallel)]
    [InlineData(AwaitOperation.ThrottleFirstLast)]
    public void SelectAwaitCancelOnCompletedTrue(AwaitOperation op)
    {
        SynchronizationContext.SetSynchronizationContext(null);

        Subject<int> subject = new Subject<int>();
        var time = new FakeTimeProvider();
        bool canceled = false;
        var list = subject.SelectAwait(async (x, ct) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), time, ct);
                }
                catch
                {
                    canceled = true;
                    throw;
                }
                return x;
            }, op, cancelOnCompleted: true)
            .ToLiveList();

        subject.OnNext(1);

        subject.OnCompleted();

        Thread.Sleep(TimeSpan.FromSeconds(1)); // CI failed?
        canceled.Should().BeTrue();
        list.AssertIsCompleted();
    }

    [Theory]
    [InlineData(AwaitOperation.Sequential)]
    [InlineData(AwaitOperation.Drop)]
    [InlineData(AwaitOperation.Switch)]
    [InlineData(AwaitOperation.Parallel)]
    [InlineData(AwaitOperation.SequentialParallel)]
    [InlineData(AwaitOperation.ThrottleFirstLast)]
    public void SelectAwaitCancelOnCompletedFalse(AwaitOperation op)
    {
        SynchronizationContext.SetSynchronizationContext(null);

        Subject<int> subject = new Subject<int>();
        var time = new FakeTimeProvider();
        bool canceled = false;
        var list = subject.SelectAwait(async (x, ct) =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), time, ct);
            }
            catch
            {
                canceled = true;
                throw;
            }
            return x * 10;
        }, op, cancelOnCompleted: false)
            .ToLiveList();

        subject.OnNext(1);

        subject.OnCompleted();

        canceled.Should().BeFalse();

        list.AssertEqual([]);
        list.AssertIsNotCompleted();

        time.Advance(3);

        list.AssertEqual([10]);
        list.AssertIsCompleted();
    }
}
