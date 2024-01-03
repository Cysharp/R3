namespace R3.Tests.FactoryTests;

public class ThrowTest
{
    // throw test
    [Fact]
    public void Throw()
    {
        {
            var e = new Exception();
            using var list = Observable.Throw<int>(e).ToLiveList();
            list.AssertEqual([]);
            list.Result.IsFailure.Should().BeTrue();
            list.Result.Exception.Should().Be(e);
        }
        {
            var fakeTime = new FakeTimeProvider();

            var e = new Exception();
            using var list = Observable.Throw<int>(e, TimeSpan.FromSeconds(5), fakeTime).ToLiveList();

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([]);
            list.Result.IsFailure.Should().BeTrue();
            list.Result.Exception.Should().Be(e);
        }
    }
}
