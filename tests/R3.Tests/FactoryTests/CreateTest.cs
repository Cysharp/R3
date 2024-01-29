using System.Runtime.CompilerServices;

namespace R3.Tests.FactoryTests;

public class CreateTest
{
    [Fact]
    public void Create()
    {
        var source = Observable.Create<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(10);
            observer.OnNext(100);
            observer.OnCompleted();
            return Disposable.Empty;
        }, rawObserver: true);

        source.ToLiveList().AssertEqual([1, 10, 100]);
    }

    [Fact]
    public void CreateS()
    {
        using var publisher = new Subject<int>();
        var source = Observable.Create<int, Subject<int>>(publisher, (observer, state) =>
        {
            return state.Subscribe(observer);
        });

        using var list = source.ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);
        publisher.OnNext(10);
        list.AssertEqual([1, 10]);
        publisher.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void AsyncCreate()
    {
        var gate = new TaskCompletionSource();

        var source = Observable.Create<int>(async (observer, _) =>
        {
            observer.OnNext(1);
            await gate.Task;
            observer.OnNext(10);
            observer.OnCompleted();
        });

        using var list = source.ToLiveList();
        list.AssertEqual([1]);
        gate.SetResult();
        list.AssertEqual([1, 10]);
    }

    [Fact]
    public void AsyncCreateCancel()
    {
        var gate = new TaskCompletionSource();

        var list = Observable.Create<int>(async (observer, ct) =>
            {
                ct.Register(() => gate.SetCanceled(ct));
                observer.OnNext(1);
                await gate.Task;
                observer.OnNext(2);
                observer.OnCompleted();
            })
            .ToLiveList();

        list.AssertEqual([1]);
        list.Dispose();
        gate.Task.Status.Should().Be(TaskStatus.Canceled);
    }

    [Fact]
    public void AsyncCreateS()
    {
        using var publisher = new Subject<int>();
        var source = Observable.Create<int, Subject<int>>(publisher,
            async (observer, state, ct) =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var value = await state.FirstAsync(ct);
                        observer.OnNext(value);
                    }
                    catch (Exception ex)
                    {
                        observer.OnCompleted(ex);
                    }
                }
            });

        using var list = source.ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);
        publisher.OnNext(10);
        list.AssertEqual([1, 10]);
        publisher.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }

    [Fact]
    public void AsyncCreateSCancel()
    {
        var gate = new TaskCompletionSource();

        var list = Observable.Create<int, TaskCompletionSource>(gate, async (observer, g, ct) =>
            {
                ct.Register(() => g.SetCanceled(ct));
                observer.OnNext(1);
                await g.Task;
                observer.OnNext(2);
                observer.OnCompleted();
            })
            .ToLiveList();

        list.AssertEqual([1]);
        list.Dispose();
        gate.Task.Status.Should().Be(TaskStatus.Canceled);
    }

    [Fact]
    public void CreateFrom()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var gate = new TaskCompletionSource();

        var source = Observable.CreateFrom(Seq);

        using var list = source.ToLiveList();
        list.AssertEqual([1]);
        gate.SetResult();
        list.AssertEqual([1, 10]);

        async IAsyncEnumerable<int> Seq([EnumeratorCancellation] CancellationToken ct)
        {
            yield return 1;
            await gate!.Task;
            yield return 10;
        }
    }

    [Fact]
    public void CreateFromCancel()
    {
        var tp = new FakeTimeProvider();

        var source = Observable.CreateFrom(Seq);

        using var list = source.ToLiveList();
        list.AssertEqual([1]);

        list.Dispose();

        async IAsyncEnumerable<int> Seq([EnumeratorCancellation] CancellationToken ct)
        {
            yield return 1;
            await Task.Delay(TimeSpan.FromSeconds(1), tp, ct);
            yield return 10;
        }
    }

    [Fact]
    public void CreateFromError()
    {
        SynchronizationContext.SetSynchronizationContext(null);

        var gate = new TaskCompletionSource();

        var source = Observable.CreateFrom(Seq);

        using var list = source.ToLiveList();
        list.AssertEqual([1]);
        gate.SetException(new Exception("foo"));
        list.Result!.Exception!.Message.Should().Be("foo");

        async IAsyncEnumerable<int> Seq([EnumeratorCancellation] CancellationToken ct)
        {
            yield return 1;
            await gate!.Task;
            yield return 10;
        }
    }
}
