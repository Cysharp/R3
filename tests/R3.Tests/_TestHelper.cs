using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace R3.Tests;

public static class _TestHelper
{
    public static void AssertEqual<T>(this LiveList<T> list, params T[] expected)
    {
        list.Should().Equal(expected);
    }

    public static void AssertIsCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.Should().BeTrue();
    }

    public static void AssertIsNotCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.Should().BeFalse();
    }
}
