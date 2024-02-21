using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class TimeIntervalFrameIntervalTimestampFrameCountTest
{
    // TimeInterval
    // FrameInterval
    // Timestamp
    // FrameCount

    [Fact]
    public void TimeInterval()
    {
        var subject = new Subject<int>();
        var provider = new FakeTimeProvider();

        using var list = subject.TimeInterval(provider).ToLiveList();

        provider.Advance(3);

        subject.OnNext(0);
        list.AssertEqual([(TimeSpan.FromSeconds(3), 0)]);

        subject.OnNext(1);
        list.AssertEqual([(TimeSpan.FromSeconds(3), 0), (TimeSpan.FromSeconds(0), 1)]);

        provider.Advance(2);

        subject.OnNext(2);
        list.AssertEqual([(TimeSpan.FromSeconds(3), 0), (TimeSpan.FromSeconds(0), 1), (TimeSpan.FromSeconds(2), 2)]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void FrameInterval()
    {
        var subject = new Subject<int>();
        var provider = new FakeFrameProvider();

        using var list = subject.FrameInterval(provider).ToLiveList();

        provider.Advance(3);

        subject.OnNext(0);
        list.AssertEqual([((3), 0)]);

        subject.OnNext(1);
        list.AssertEqual([((3), 0), ((0), 1)]);

        provider.Advance(2);

        subject.OnNext(2);
        list.AssertEqual([((3), 0), ((0), 1), ((2), 2)]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void Timestamp()
    {
        var subject = new Subject<int>();
        var provider = new FakeTimeProvider(DateTimeOffset.MinValue);

        using var list = subject.Timestamp(provider).ToLiveList();

        subject.OnNext(0);
        list.AssertEqual([(0, 0)]);

        provider.Advance(TimeSpan.FromTicks(3));
        subject.OnNext(1);
        list.AssertEqual([(0, 0), (3, 1)]);

        provider.Advance(TimeSpan.FromTicks(2));

        subject.OnNext(2);
        list.AssertEqual([(0, 0), (3, 1), (5, 2)]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void FrameCount()
    {
        var subject = new Subject<int>();
        var provider = new FakeFrameProvider();

        using var list = subject.FrameCount(provider).ToLiveList();

        subject.OnNext(0);
        list.AssertEqual([(0, 0)]);

        provider.Advance(3);
        subject.OnNext(1);
        list.AssertEqual([(0, 0), (3, 1)]);

        provider.Advance(2);

        subject.OnNext(2);
        list.AssertEqual([(0, 0), (3, 1), (5, 2)]);

        subject.OnCompleted();

        list.AssertIsCompleted();
    }
}
