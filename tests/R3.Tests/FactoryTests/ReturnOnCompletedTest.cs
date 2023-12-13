
namespace R3.Tests.FactoryTests;

public class ReturnOnCompletedTest
{
    // test
    [Fact]
    public void ReturnOnCompleted()
    {
        {
            using var list = Event.ReturnOnCompleted<int, string>("foo").ToLiveList();
            list.AssertEqual([]);
            list.AssertIsCompleted();
            list.AssertCompletedValue("foo");
        }
        {
            var fakeTime = new FakeTimeProvider();

            using var list = Event.ReturnOnCompleted<int, string>("foo", TimeSpan.FromSeconds(5), fakeTime).ToLiveList();

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([]);
            list.AssertIsCompleted();
            list.AssertCompletedValue("foo");
        }
    }
}
