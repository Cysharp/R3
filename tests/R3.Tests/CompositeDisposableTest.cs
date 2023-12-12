namespace R3.Tests;

public class CompositeDisposableTest
{
    [Fact]
    public void Add()
    {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();

        var composite = new CompositeDisposable();

        composite.Add(d1);
        composite.Add(d2);
        composite.Add(d3);

        d1.CalledCount.Should().Be(0);

        composite.Remove(d2);
        d2.CalledCount.Should().Be(1);

        composite.Clear();
        d1.CalledCount.Should().Be(1);
        d3.CalledCount.Should().Be(1);

        composite.Add(d1);
        composite.Add(d2);
        composite.Add(d3);

        composite.Dispose();

        d1.CalledCount.Should().Be(2);
        d2.CalledCount.Should().Be(2);
        d3.CalledCount.Should().Be(2);

        composite.Add(d1);
        d1.CalledCount.Should().Be(3);
    }

    [Fact]
    public void RemoveAndShrink()
    {
        var disposables = Enumerable.Range(1, 100).Select(x => new TestDisposable()).ToArray();
        var composite = new CompositeDisposable(disposables);

        foreach (var item in disposables)
        {
            composite.Remove(item);
        }

        foreach (var item in disposables)
        {
            item.CalledCount.Should().Be(1);
        }
    }


    class TestDisposable : IDisposable
    {
        public int CalledCount = 0;

        public void Dispose()
        {
            CalledCount += 1;
        }
    }
}
