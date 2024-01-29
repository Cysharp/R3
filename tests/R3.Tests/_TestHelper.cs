using R3.Collections;

namespace R3.Tests;



public static class _TestHelper
{
    public static void AssertEqual<T>(this LiveList<T> list, params T[] expected)
    {
        list.Should().Equal(expected);
    }

    public static void AssertEqual<T>(this LiveList<T[]> list, params T[][] expected)
    {
        list.Count.Should().Be(expected.Length);

        for (int i = 0; i < expected.Length; i++)
        {
            list[i].Should().Equal(expected[i]);
        }
    }

    public static void AssertEmpty<T>(this LiveList<T> list)
    {
        list.Count.Should().Be(0);
    }

    public static void AssertIsCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.Should().BeTrue();
    }

    public static void AssertIsNotCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.Should().BeFalse();
    }

    public static void Advance(this FakeTimeProvider timeProvider, int seconds)
    {
        timeProvider.Advance(TimeSpan.FromSeconds(seconds));
    }
}
