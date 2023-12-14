using System.Collections.Generic;

namespace R3.Tests.FactoryTests;

public class RangeTest
{
    [Fact]
    public void Range()
    {
        using var list1 = Event.Range(5, 8).ToLiveList();
        list1.AssertEqual([5, 6, 7, 8, 9, 10, 11, 12]);
        list1.AssertIsCompleted();

        using var list2 = Event.Range(20, 3).ToLiveList();
        list2.AssertEqual([20, 21, 22]);
        list2.AssertIsCompleted();

        using var list3 = Event.Range(-3, 5).ToLiveList();
        list3.AssertEqual([-3, -2, -1, 0, 1]);
        list3.AssertIsCompleted();

        using var list4 = Event.Range(10, 0).ToLiveList();
        list4.AssertEqual([]);
        list4.AssertIsCompleted();

        Assert.Throws<ArgumentOutOfRangeException>(() => Event.Range(10, -1));
    }

    [Fact]
    public void Stop()
    {
        var cts = new CancellationTokenSource();

        using var list = Event.Range(0, int.MaxValue, cts.Token)
            .Take(5)
            .CancelOnCompleted(cts)
            .ToLiveList();

        list.AssertEqual([0, 1, 2, 3, 4]);
        list.AssertIsCompleted();
    }
}
