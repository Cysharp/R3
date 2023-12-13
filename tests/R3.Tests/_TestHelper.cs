using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace R3.Tests;

public static class _TestHelper
{
    public static void AssertEqual<T, TC>(this LiveList<T, TC> list, params T[] expected)
    {
        list.Should().Equal(expected);
    }

    public static void AssertIsCompleted<T, TC>(this LiveList<T, TC> list)
    {
        list.IsCompleted.Should().BeTrue();
    }

    public static void AssertIsNotCompleted<T, TC>(this LiveList<T, TC> list)
    {
        list.IsCompleted.Should().BeFalse();
    }

    public static void AssertCompletedValue<T, TC>(this LiveList<T, TC> list, TC value)
    {
        list.CompletedValue.Should().Be(value);
    }
}
