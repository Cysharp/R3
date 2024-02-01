using System.Runtime.CompilerServices;

namespace R3.Tests.FactoryTests;

public class ToObservableTest
{
    [Fact]
    public async Task TaskToObservable()
    {
        var fakeTime = new FakeTimeProvider();
        var t = System.Threading.Tasks.Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1), fakeTime);
            return 100;
        });
        var list = t.ToObservable().ToLiveList();

        list.AssertIsNotCompleted();

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        await t;
        await Task.Delay(1); // wait

        list.AssertIsCompleted();
        list.AssertEqual([100]);
    }

    [Fact]
    public void EnumerableToObservable()
    {
        {
            var list = Enumerable.Range(0, 10).ToObservable().ToLiveList();

            list.AssertEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
            list.AssertIsCompleted();
        }
        {
            var cts = new CancellationTokenSource();

            var list = Enumerable.Range(0, int.MaxValue)
                .ToObservable(cts.Token)
                .Take(10)
                .DoCancelOnCompleted(cts)
                .ToLiveList();

            list.AssertEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void ObservableToObservable()
    {
        var l1 = new SuccessObservable().ToObservable().ToLiveList();
        l1.AssertEqual([1, 2, 3]);
        l1.AssertIsCompleted();

        var l2 = new FaileObservable().ToObservable().ToLiveList();
        l2.AssertEqual([1, 2, 3]);
        l2.AssertIsCompleted();
        l2.Result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncEnumerableToObservable()
    {
        var fakeTime = new FakeTimeProvider();
        using var list = AsyncOne(fakeTime).ToObservable().ToLiveList();

        list.AssertEqual([1]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));

        await Task.Delay(1000);
        await Task.Yield();
        list.AssertEqual([1, 2]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));

        await Task.Delay(1000);
        await Task.Yield();
        list.AssertEqual([1, 2, 3]);
    }

    [Fact]
    public async Task AsyncEnumerableToObservableCt()
    {
        var fakeTime = new FakeTimeProvider();
        using var list = AsyncOne(fakeTime).ToObservable().ToLiveList();

        list.AssertEqual([1]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));

        await Task.Delay(1000);
        await Task.Yield();
        list.AssertEqual([1, 2]);

        list.Dispose();
    }

    [Fact]
    public async Task AsyncEnumerableToObservableEx()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var fakeTime = new FakeTimeProvider();
        using var list = AsyncTwo(fakeTime).ToObservable().ToLiveList();

        list.AssertEqual([1]);

        fakeTime.Advance(TimeSpan.FromSeconds(1));

        await Task.Delay(1000);
        await Task.Yield();
        list.AssertEqual([1, 2]);

        list.AssertIsNotCompleted();
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        list.Result.IsFailure.Should().BeTrue();
        list.Result.Exception!.Message.Should().Be("foo");
    }

    async IAsyncEnumerable<int> AsyncOne(TimeProvider timeProvider, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return 1;
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, cancellationToken);
        yield return 2;
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, cancellationToken);
        yield return 3;
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, cancellationToken);
    }

    async IAsyncEnumerable<int> AsyncTwo(TimeProvider timeProvider, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return 1;
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, cancellationToken);
        yield return 2;
        await Task.Delay(TimeSpan.FromSeconds(1), timeProvider, cancellationToken);
        throw new Exception("foo");
    }

    class SuccessObservable : IObservable<int>
    {
        public IDisposable Subscribe(IObserver<int> observer)
        {
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }

    class FaileObservable : IObservable<int>
    {
        public IDisposable Subscribe(IObserver<int> observer)
        {
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);
            observer.OnError(new Exception());
            return Disposable.Empty;
        }
    }

    [Fact]
    public void TaskToObservable2()
    {
        var tcs = new TaskCompletionSource();

        var myLiveList = tcs.Task.ToObservable()
            .Select(x => x)
            .Where(x => true)
            .Take(10)
            .ToLiveList();

        myLiveList.Dispose();

        tcs.TrySetResult();
    }
}
