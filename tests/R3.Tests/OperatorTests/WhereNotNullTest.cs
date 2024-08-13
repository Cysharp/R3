namespace R3.Tests.OperatorTests;

public class WhereNotNullTest(ITestOutputHelper output)
{
    [Fact]
    public void WhereNotNull()
    {
        var p = new Subject<string?>();

        using var list = p.WhereNotNull().ToLiveList();

        p.OnNext(null);
        list.AssertEqual([]);

        p.OnNext("foo");
        list.AssertEqual(["foo"]);

        p.OnNext(null);
        list.AssertEqual(["foo"]);

        p.OnNext("bar");
        list.AssertEqual(["foo", "bar"]);
    }
}
