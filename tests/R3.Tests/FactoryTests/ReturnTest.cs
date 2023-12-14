using Microsoft.Extensions.Time.Testing;

namespace R3.Tests.FactoryTests;

public class ReturnTest
{
    [Fact]
    public void Return()
    {
        {
            using var list = Event.Return(10).ToLiveList();
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Event.Return(10, TimeSpan.Zero, fakeTime).ToLiveList();
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Event.Return(10, TimeSpan.FromSeconds(5), fakeTime).ToLiveList();
            list.AssertEqual([]);

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNoResultd();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([10]);
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void ReturnThreadPoolScheduleOptimized()
    {
        using var list = Event.Return(10).ToLiveList();

        Thread.Sleep(1);

        list.AssertEqual([10]);
        list.AssertIsCompleted();
    }

    // return on completed
    [Fact]
    public void ReturnOnCompleted()
    {
        {
            using var list = Event.Return(0, "foo").ToLiveList();
            list.AssertEqual([0]);
            list.AssertIsCompleted();
            list.AsserResultdValue("foo");
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Event.Return(10, "foo", TimeSpan.FromSeconds(5), fakeTime).ToLiveList();
            list.AssertEqual([]);

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNoResultd();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([10]);
            list.AssertIsCompleted();
            list.AsserResultdValue("foo");
        }
    }
}
