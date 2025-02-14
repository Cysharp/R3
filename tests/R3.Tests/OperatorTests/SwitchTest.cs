using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class SwitchTest
{
    [Fact]
    public void Switch()
    {
        var sources = new Subject<Observable<int>>();

        using var list = sources.Switch().ToLiveList();

        var source1 = new Subject<int>();
        var source2 = new Subject<int>();

        sources.OnNext(source1);

        list.AssertEmpty();

        source1.OnNext(1);
        source1.OnNext(2);

        sources.OnNext(source2);

        source2.OnNext(10);

        source1.OnNext(3);

        list.AssertEqual([1, 2, 10]);

        source1.OnCompleted();
        source2.OnCompleted();

        list.AssertIsNotCompleted();

        sources.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void DisposeOrderCheck()
    {
        var sources = new Subject<Observable<int>>();

        using var list = sources.Switch().ToLiveList();

        var source1 = new Subject<int>();
        var source2 = new Subject<int>();

        var msgs = new List<string>();

        sources.OnNext(source1.Do(onDispose: () => msgs.Add("dispose source1"), onSubscribe: () => msgs.Add("subscribe source1")));
        sources.OnNext(source2.Do(onDispose: () => msgs.Add("dispose source2"), onSubscribe: () => msgs.Add("subscribe source2")));
        source2.OnCompleted();

        msgs.Is(["subscribe source1", "dispose source1", "subscribe source2", "dispose source2"]);
    }
}
