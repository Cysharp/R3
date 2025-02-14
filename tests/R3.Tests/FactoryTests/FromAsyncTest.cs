namespace R3.Tests.FactoryTests;

public class FromAsyncTest
{
    [Fact]
    public void FromAsync()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var fakeTime = new FakeTimeProvider();
        var list = Observable.FromAsync(async (ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1), fakeTime, ct);
            return 1000;
        }).ToLiveList();

        list.AssertEqual([]);

        fakeTime.Advance(1);

        list.AssertEqual([1000]);
        list.AssertIsCompleted();
    }

    [Fact]
    public async Task FromAsyncCancel()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        var fakeTime = new FakeTimeProvider();
        var cancelled = new TaskCompletionSource();
        var list = Observable.FromAsync(async (ct) =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), fakeTime, ct);
            }
            catch (OperationCanceledException)
            {
                cancelled.TrySetResult();
                throw;
            }
            return 1000;
        }).ToLiveList();

        list.AssertEqual([]);

        list.Dispose();

        await cancelled.Task; // await OK.
    }
}
