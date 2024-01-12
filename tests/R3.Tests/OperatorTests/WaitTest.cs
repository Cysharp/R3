namespace R3.Tests.OperatorTests;

public class WaitTest
{
    [Fact]
    public async Task AnyValues()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        await source.WaitAsync();

        var p = new Subject<int>();
        var task = p.WaitAsync();

        p.OnNext(10);
        p.OnNext(20);
        p.OnNext(30);
        p.OnCompleted();

        await task;
    }
}
