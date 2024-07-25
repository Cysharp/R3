using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class TakeUntilTest
{
    [Fact]
    public void EventOther()
    {
        var publisher1 = new Subject<int>();
        var publisher2 = new Subject<int>();
        var isDisposed = false;
        var list = publisher1.TakeUntil(publisher2.Do(onDispose: () => { isDisposed = true; })).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        publisher2.OnNext(10000);
        isDisposed.Should().BeTrue();

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }


    [Fact]
    public void CancellationToken()
    {
        var publisher1 = new Subject<int>();
        var cts = new CancellationTokenSource();
        var list = publisher1.TakeUntil(cts.Token).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        cts.Cancel();

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }

    [Fact]
    public async Task TaskT()
    {
        var publisher1 = new Subject<int>();
        var tcs = new TaskCompletionSource();
        var list = publisher1.TakeUntil(tcs.Task).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        tcs.TrySetResult();
        await Task.Delay(100); // wait for completion

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }

    [Fact]
    public void Async()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher1 = new Subject<int>();
        var tcs = new TaskCompletionSource();
        var list = publisher1.TakeUntil(async (x, ct) => await tcs.Task).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([1, 2, 3]);

        tcs.TrySetResult();

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();

    }

    [Fact]
    public void Predicate()
    {
        var sequence = new[] { "A", "B", "C", "D", "E", "F", "G" };

        sequence.ToObservable().TakeUntil(x => x == "E").ToLiveList().AssertEqual("A", "B", "C", "D", "E");
        sequence.ToObservable().TakeUntil(x => x == "A").ToLiveList().AssertEqual("A");

        sequence.ToObservable().TakeUntil((x, i) => i == 4).ToLiveList().AssertEqual("A", "B", "C", "D", "E");
        sequence.ToObservable().TakeUntil((x, i) => i == 0).ToLiveList().AssertEqual("A");
    }
}
