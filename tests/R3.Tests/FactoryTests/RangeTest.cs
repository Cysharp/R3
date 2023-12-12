using System.Collections.Generic;

namespace R3.Tests.FactoryTests;

public class RangeTest
{
    [Fact]
    public void Range()
    {
        using var list1 = EventFactory.Range(5, 8).ToLiveList();
        list1.AssertEqual([5, 6, 7, 8, 9, 10, 11, 12]);
        list1.AssertIsCompleted();

        using var list2 = EventFactory.Range(20, 3).ToLiveList();
        list2.AssertEqual([20, 21, 22]);
        list2.AssertIsCompleted();

        using var list3 = EventFactory.Range(-3, 5).ToLiveList();
        list3.AssertEqual([-3, -2, -1, 0, 1]);
        list3.AssertIsCompleted();

        using var list4 = EventFactory.Range(10, 0).ToLiveList();
        list4.AssertEqual([]);
        list4.AssertIsCompleted();

        Assert.Throws<ArgumentOutOfRangeException>(() => EventFactory.Range(10, -1));
    }

    [Fact]
    public void Stop()
    {
        var cts = new CancellationTokenSource();

        using var list = EventFactory.Range(0, int.MaxValue, cts.Token)
            .Take(5)
            .DoOnCompleted(() => cts.Cancel())
            .ToLiveList();

        list.AssertEqual([0, 1, 2, 3, 4]);
        list.AssertIsCompleted();
    }
}
