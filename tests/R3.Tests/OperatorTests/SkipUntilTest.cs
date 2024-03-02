using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class SkipUntilTest
{
    [Fact]
    public void EventOther()
    {
        var publisher1 = new Subject<int>();
        var publisher2 = new Subject<int>();
        var isDisposed = false;
        var list = publisher1.SkipUntil(publisher2.Do(onDispose: () => { isDisposed = true; })).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([]);

        publisher2.OnNext(10000);
        isDisposed.Should().BeTrue();

        publisher1.OnNext(999999);
        publisher1.OnNext(9999990);

        list.AssertEqual([999999, 9999990]);
        publisher1.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void CancellationToken()
    {
        var publisher1 = new Subject<int>();
        var cts = new CancellationTokenSource();
        var list = publisher1.SkipUntil(cts.Token).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([]);

        cts.Cancel();

        publisher1.OnNext(999999);
        publisher1.OnNext(9999990);

        list.AssertEqual([999999, 9999990]);
        publisher1.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public async Task TaskT()
    {
        var publisher1 = new Subject<int>();
        var tcs = new TaskCompletionSource();
        var list = publisher1.SkipUntil(tcs.Task).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([]);

        tcs.TrySetResult();
        await Task.Delay(100); // wait for completion


        publisher1.OnNext(999999);
        publisher1.OnNext(9999990);

        list.AssertEqual([999999, 9999990]);
        publisher1.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void Async()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var publisher1 = new Subject<int>();
        var tcs = new TaskCompletionSource();
        var list = publisher1.SkipUntil(async (x,ct) => await tcs.Task).ToLiveList();

        publisher1.OnNext(1);
        publisher1.OnNext(2);
        publisher1.OnNext(3);
        list.AssertEqual([]);

        tcs.TrySetResult();

        publisher1.OnNext(999999);
        publisher1.OnNext(9999990);

        list.AssertEqual([999999, 9999990]);
        publisher1.OnCompleted();
        list.AssertIsCompleted();
    }
}
