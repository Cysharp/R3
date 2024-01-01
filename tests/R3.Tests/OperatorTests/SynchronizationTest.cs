namespace R3.Tests.OperatorTests;

public class SynchronizationTest(ITestOutputHelper output)
{
    [Fact]
    public void Test()
    {
        var subject = new Subject<int>();

        var count = 0;
        var no_sync = 0;
        subject.Subscribe(x => no_sync++);
        subject.Synchronize().Subscribe(x => count++);

        Parallel.For(0, 100, x => subject.OnNext(x));

        count.Should().Be(100);
        output.WriteLine($"Count: {count}, no_sync: {no_sync}");
    }
}
