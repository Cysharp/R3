namespace R3.Tests;

public class SerialDisposableTest
{
    [Fact]
    public void Dispose()
    {
        var l = new List<int>();

        var d = new SerialDisposableCore();
        d.Disposable = Disposable.Create(() => l.Add(1));

        l.Should().Equal([]);

        d.Disposable = Disposable.Create(() => l.Add(2));
        l.Should().Equal([1]);

        d.Disposable = Disposable.Create(() => l.Add(3));
        l.Should().Equal([1, 2]);

        d.Disposable = Disposable.Create(() => l.Add(4));
        l.Should().Equal([1, 2, 3]);

        d.Dispose();

        l.Should().Equal([1, 2, 3, 4]);

        d.Disposable = Disposable.Create(() => l.Add(5));

        l.Should().Equal([1, 2, 3, 4, 5]);
    }
}
