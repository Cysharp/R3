using System.Reactive.Linq;

namespace R3.Tests.OperatorTests;

public class TakeUntilTest
{
    [Fact]
    public void EventOther()
    {
        var publisher1 = new Publisher<int>();
        var publisher2 = new Publisher<int>();
        var isDisposed = false;
        var list = publisher1.TakeUntil(publisher2.DoOnDisposed(() => { isDisposed = true; })).ToLiveList();

        publisher1.PublishOnNext(1);
        publisher1.PublishOnNext(2);
        publisher1.PublishOnNext(3);
        list.AssertEqual([1, 2, 3]);

        publisher2.PublishOnNext(10000);
        isDisposed.Should().BeTrue();

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }


    [Fact]
    public void CancellationToken()
    {
        var publisher1 = new Publisher<int>();
        var cts = new CancellationTokenSource();
        var list = publisher1.TakeUntil(cts.Token).ToLiveList();

        publisher1.PublishOnNext(1);
        publisher1.PublishOnNext(2);
        publisher1.PublishOnNext(3);
        list.AssertEqual([1, 2, 3]);

        cts.Cancel();

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }

    [Fact]
    public async Task TaskT()
    {
        var publisher1 = new Publisher<int>();
        var tcs = new TaskCompletionSource();
        var list = publisher1.TakeUntil(tcs.Task).ToLiveList();

        publisher1.PublishOnNext(1);
        publisher1.PublishOnNext(2);
        publisher1.PublishOnNext(3);
        list.AssertEqual([1, 2, 3]);

        tcs.TrySetResult();
        await Task.Delay(100); // wait for completion

        list.AssertEqual([1, 2, 3]);
        list.AssertIsCompleted();
    }
}
