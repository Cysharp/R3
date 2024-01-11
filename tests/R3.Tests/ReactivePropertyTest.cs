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
}
