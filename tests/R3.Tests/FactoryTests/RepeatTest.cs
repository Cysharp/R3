namespace R3.Tests.FactoryTests;

public class RepeatTest
{
    // test
    [Fact]
    public void Repeat()
    {
        using var list = Observable.Repeat("foo", 3).ToLiveList();
        list.AssertEqual(["foo", "foo", "foo"]);
        list.AssertIsCompleted();

        using var list2 = Observable.Repeat("foo", 0).ToLiveList();
        list2.AssertEqual([]);
        list2.AssertIsCompleted();

        Assert.Throws<ArgumentOutOfRangeException>(() => Observable.Repeat("foo", -1));
    }

    [Fact]
    public void Stop()
    {
        var cts = new CancellationTokenSource();

        using var list = Observable.Repeat("foo", int.MaxValue, cts.Token)
            .Take(5)
            .DoCancelOnCompleted(cts)
            .ToLiveList();

        list.AssertEqual(["foo", "foo", "foo", "foo", "foo"]);
        list.AssertIsCompleted();
    }
}
