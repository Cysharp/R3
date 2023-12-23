using Microsoft.Extensions.Time.Testing;

namespace R3.Tests.FactoryTests;

public class ReturnTest
{
    [Fact]
    public void Return()
    {
        {
            using var list = Observable.Return(10).ToLiveList();
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Observable.Return(10, TimeSpan.Zero, fakeTime).ToLiveList();
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Observable.Return(10, TimeSpan.FromSeconds(5), fakeTime).ToLiveList();
            list.AssertEqual([]);

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void ReturnThreadPoolScheduleOptimized()
    {
        using var list = Observable.Return(10).ToLiveList();

        Thread.Sleep(1);

        list.AssertEqual([10]);
        list.AssertIsCompleted();
    }

    // return on completed
    [Fact]
    public void ReturnOnCompleted()
    {
        {
            using var list = Observable.Return(0).ToLiveList();
            list.AssertEqual([0]);
            list.AssertIsCompleted();
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Observable.Return(10, TimeSpan.FromSeconds(5), fakeTime).ToLiveList();
            list.AssertEqual([]);

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void Optimized()
    {
        for (int i = -10; i < 100; i++)
        {
            using var list = Observable.Return(i).ToLiveList(); // int optimized
            list.AssertEqual([i]);
            list.AssertIsCompleted();
        }

        foreach (var item in new bool[] { true, false })
        {
            using var list = Observable.Return(item).ToLiveList(); // bool optimized
            list.AssertEqual([item]);
            list.AssertIsCompleted();
        }

        Observable.Return(Unit.Default).ToLiveList().AssertEqual([Unit.Default]);
        Observable.ReturnUnit().ToLiveList().AssertEqual([Unit.Default]);
    }
}
