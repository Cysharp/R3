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

        d1.CalledCount.ShouldBe(0);

        composite.Remove(d2);
        d2.CalledCount.ShouldBe(1);

        composite.Clear();
        d1.CalledCount.ShouldBe(1);
        d3.CalledCount.ShouldBe(1);

        composite.Add(d1);
        composite.Add(d2);
        composite.Add(d3);

        composite.Dispose();

        d1.CalledCount.ShouldBe(2);
        d2.CalledCount.ShouldBe(2);
        d3.CalledCount.ShouldBe(2);

        composite.Add(d1);
        d1.CalledCount.ShouldBe(3);
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
            item.CalledCount.ShouldBe(1);
        }
    }

    /// <summary>
    /// Remove nulls out the slot instead of compacting the backing list, so this guarantees
    /// that CopyTo skips those holes and writes the surviving disposables contiguously
    /// starting at arrayIndex.
    /// </summary>
    [Fact]
    public void CopyToSkipsRemovedSlots()
    {
        var disposables = Enumerable.Range(0, 5).Select(x => new TestDisposable()).ToArray();
        var composite = new CompositeDisposable(disposables);

        composite.Remove(disposables[1]);
        composite.Remove(disposables[3]);

        composite.Count.ShouldBe(3);

        var array = new IDisposable[4];
        composite.CopyTo(array, 1);

        array[0].ShouldBeNull();
        array[1].ShouldBe(disposables[0]);
        array[2].ShouldBe(disposables[2]);
        array[3].ShouldBe(disposables[4]);
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
