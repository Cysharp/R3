using System.Text.Json;

namespace R3.Tests;

public class BindableReactivePropertyTest
{
    [Fact]
    public void Test()
    {
        var rp = new ReactiveProperty<int>(100);
        var brp = rp.ToBindableReactiveProperty();

        var list = brp.ToLiveList();
        list.AssertEqual([100]);

        rp.Value = 9999;

        var list2 = brp.ToLiveList();
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
    public void ReadOnlyTest()
    {
        var rp = new ReactiveProperty<int>(100);
        var rorp = rp.ToReadOnlyReactiveProperty();
        var brp0 = rp.ToReadOnlyBindableReactiveProperty();
        var brp1 = rorp.ToReadOnlyBindableReactiveProperty();

        brp0.CurrentValue.Should().Be(100);
        brp1.CurrentValue.Should().Be(100);
        var list0 = brp0.ToLiveList();
        var list1 = brp1.ToLiveList();
        list0.AssertEqual([100]);
        list1.AssertEqual([100]);

        rp.Value = 9999;

        brp0.CurrentValue.Should().Be(9999);
        brp1.CurrentValue.Should().Be(9999);
        list0.AssertEqual([100, 9999]);
        list1.AssertEqual([100, 9999]);

        rp.Dispose();
        list0.AssertIsCompleted();
        list1.AssertIsCompleted();
    }

    [Fact]
    public void DefaultValueTest()
    {
        using var rp = new ReactiveProperty<int>();
        var brp0 = rp.ToBindableReactiveProperty();
        var brp1 = rp.ToReadOnlyBindableReactiveProperty();
        brp0.Value.Should().Be(default);
        brp1.CurrentValue.Should().Be(default);
    }

    [Fact]
    public void SubscribeAfterCompleted()
    {
        var rp = new ReactiveProperty<string>("foo");
        var brp0 = rp.ToBindableReactiveProperty();
        var brp1 = rp.ToReadOnlyBindableReactiveProperty();
        rp.OnCompleted();

        using var list0 = brp0.ToLiveList();
        using var list1 = brp1.ToLiveList();

        list0.AssertIsCompleted();
        list0.AssertEqual(["foo"]);
        list1.AssertIsCompleted();
        list1.AssertEqual(["foo"]);
    }

    [Fact]
    public void SerializeTest()
    {
        var rp = new ReactiveProperty<string>("foo");
        var brp0 = rp.ToBindableReactiveProperty("");
        var brp1 = rp.ToReadOnlyBindableReactiveProperty("");

        JsonSerializer.Serialize(brp0).Should().Be("\"foo\"");
        JsonSerializer.Serialize(brp1).Should().Be("""{"EqualityComparer":{},"CurrentValue":"foo","IsDisposed":false}""");
    }
}
