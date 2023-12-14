namespace R3.Tests.FactoryTests;

public class EveryUpdateTest
{
    [Fact]
    public void EveryUpdate()
    {
        var cts = new CancellationTokenSource();
        var frameProvider = new ManualFrameProvider();

        var list = Event.EveryUpdate(frameProvider, cts.Token).Select(_ => frameProvider.GetFrameCount()).ToLiveList();

        list.AssertEqual([]);

        frameProvider.Advance();
        list.AssertEqual([0]);

        frameProvider.Advance(3);
        list.AssertEqual([0, 1, 2, 3]);

        cts.Cancel();
        list.AssertIsNotCompleted();

        frameProvider.Advance();
        list.AssertIsCompleted();
    }

    [Fact]
    public void EveryUpdateCancelImmediate()
    {
        var cts = new CancellationTokenSource();
        var frameProvider = new ManualFrameProvider();

        var list = Event.EveryUpdate(frameProvider, cts.Token, cancelImmediately: true).Select(_ => frameProvider.GetFrameCount()).ToLiveList();

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
        var frameProvider = new ManualFrameProvider();

        var list = Event.EveryUpdate(frameProvider).Select(_ => frameProvider.GetFrameCount()).ToLiveList();

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
