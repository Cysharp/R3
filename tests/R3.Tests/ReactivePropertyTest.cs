namespace R3.Tests;

public class ReactivePropertyTest
{
    [Fact]
    public void Test()
    {
        var rp = new ReactiveProperty<int>(100);
        rp.Value.Should().Be(100);

        var list = rp.ToLiveList();
        list.AssertEqual([100]);

        rp.Value = 9999;

        var list2 = rp.ToLiveList();
        list.AssertEqual([100, 9999]);
        list2.AssertEqual([9999]);

        rp.Value = 9999;
        list.AssertEqual([100, 9999]);
        list2.AssertEqual([9999]);

        rp.Value = 100;
        list.AssertEqual([100, 9999, 100]);
        list2.AssertEqual([9999, 100]);

        rp.Dispose();

        list.AssertIsCompleted();
        list2.AssertIsCompleted();
    }

    [Fact]
    public void DefaultValueTest()
    {
        using var rp = new ReactiveProperty<int>();
        rp.Value.Should().Be(default);
    }

    [Fact]
    public void SubscribeAfterCompleted()
    {
        var rp = new ReactiveProperty<string>("foo");
        rp.OnCompleted();

        using var list = rp.ToLiveList();

        list.AssertIsCompleted();
        list.AssertEqual(["foo"]);
    }

    // Node Check
    [Fact]
    public void CheckNode()
    {
        var rp = new ReactiveProperty<int>();


        var list1 = rp.ToLiveList();

        rp.Value = 1;

        list1.AssertEqual([0, 1]);

        list1.Dispose();

        var list2 = rp.ToLiveList();

        rp.Value = 2;

        list2.AssertEqual([1, 2]);

        var list3 = rp.ToLiveList();
        var list4 = rp.ToLiveList();

        // remove first

        list2.Dispose();
        rp.Value = 3;

        list3.AssertEqual([2, 3]);
        list4.AssertEqual([2, 3]);

        var list5 = rp.ToLiveList();

        // remove middle
        list4.Dispose();

        rp.Value = 4;

        list3.AssertEqual([2, 3, 4]);
        list5.AssertEqual([3, 4]);

        // remove last
        list5.Dispose();

        rp.Value = 5;
        list3.AssertEqual([2, 3, 4, 5]);

        // remove single
        list3.Dispose();

        // subscribe once
        var list6 = rp.ToLiveList();
        rp.Value = 6;
        list6.AssertEqual([5, 6]);
    }
}
