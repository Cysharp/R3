namespace R3.Tests.FactoryTests;

public class ThrowTest
{
    // throw test
    [Fact]
    public void Throw()
    {
        {
            var e = new Exception();
            using var list = EventFactory.Throw<int>(e).LiveRecord();
            list.AssertEqual([]);
            list.CompletedValue.IsFailure.Should().BeTrue();
            list.CompletedValue.Exception.Should().Be(e);
        }
        {
            var fakeTime = new FakeTimeProvider();

            var e = new Exception();
            using var list = EventFactory.Throw<int>(e, TimeSpan.FromSeconds(5), fakeTime).LiveRecord();

            fakeTime.Advance(TimeSpan.FromSeconds(4));
            list.AssertEqual([]);
            list.AssertIsNotCompleted();

            fakeTime.Advance(TimeSpan.FromSeconds(1));
            list.AssertEqual([]);
            list.CompletedValue.IsFailure.Should().BeTrue();
            list.CompletedValue.Exception.Should().Be(e);
        }
    }
}
