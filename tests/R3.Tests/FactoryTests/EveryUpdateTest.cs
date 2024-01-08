namespace R3.Tests.FactoryTests;

public class EveryUpdateTest
{
    [Fact]
    public void EveryUpdateCancelImmediate()
    {
        var cts = new CancellationTokenSource();
        var frameProvider = new FakeFrameProvider();

        var list = Observable.EveryUpdate(frameProvider, cts.Token).Select(_ => frameProvider.GetFrameCount()).ToLiveList();

        list.AssertEqual([]);

        frameProvider.Advance();
        list.AssertEqual([0]);

        frameProvider.Advance(3);
        list.AssertEqual([0, 1, 2, 3]);

        cts.Cancel();
        list.AssertIsCompleted();

        frameProvider.Advance();
        list.AssertEqual([0, 1, 2, 3]);
        list.AssertIsCompleted();
    }
    
    [Fact]
    public void EveryUpdateDispose()
    {
        var frameProvider = new FakeFrameProvider();

        var list = Observable.EveryUpdate(frameProvider).Select(_ => frameProvider.GetFrameCount()).ToLiveList();

        list.AssertEqual([]);

        frameProvider.Advance();
        list.AssertEqual([0]);

        frameProvider.Advance(3);
        list.AssertEqual([0, 1, 2, 3]);

        list.Dispose();
        frameProvider.Advance();
        list.AssertEqual([0, 1, 2, 3]);
    }
}
