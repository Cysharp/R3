using R3.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace R3.Tests;

public static class _TestHelper
{
    public static void AssertEqual<T>(this LiveList<T> list, params T[] expected)
    {
        list.ShouldBe(expected);
    }

    public static void AssertEqual<T>(this LiveList<T[]> list, params T[][] expected)
    {
        list.Count.ShouldBe(expected.Length);

        for (int i = 0; i < expected.Length; i++)
        {
            list[i].ShouldBe(expected[i]);
        }
    }

    public static void AssertEmpty<T>(this LiveList<T> list)
    {
        list.Count.ShouldBe(0);
    }

    public static void AssertIsCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.ShouldBeTrue();
    }

    public static void AssertIsNotCompleted<T>(this LiveList<T> list)
    {
        list.IsCompleted.ShouldBeFalse();
    }

    public static void Advance(this FakeTimeProvider timeProvider, int seconds)
    {
        timeProvider.Advance(TimeSpan.FromSeconds(seconds));
    }

    // ShouldBe -> Is

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Is<T>(
       [NotNullIfNotNull(nameof(expected))] this T? actual,
       [NotNullIfNotNull(nameof(actual))] T? expected,
       string? customMessage = null)
    {
        actual.ShouldBe(expected, customMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Is<T>(
        [NotNullIfNotNull(nameof(expected))] this T? actual,
        [NotNullIfNotNull(nameof(actual))] T? expected,
        IEqualityComparer<T> comparer,
        string? customMessage = null)
    {
        actual.ShouldBe(expected, comparer, customMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void IsNot<T>(this T? actual, T? expected, string? customMessage = null)
    {
        actual.ShouldNotBe(expected, customMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void IsNot<T>(this T? actual, T? expected, IEqualityComparer<T> comparer, string? customMessage = null)
    {
        actual.ShouldNotBe(expected, comparer, customMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Is<T>(
        [NotNullIfNotNull(nameof(expected))] this IEnumerable<T>? actual,
        [NotNullIfNotNull(nameof(actual))] IEnumerable<T>? expected,
        bool ignoreOrder = false)
    {
        actual.ShouldBe(expected, ignoreOrder);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Is<T>(
        [NotNullIfNotNull(nameof(expected))] this IEnumerable<T>? actual,
        [NotNullIfNotNull(nameof(actual))] IEnumerable<T>? expected,
        bool ignoreOrder,
        string? customMessage)
    {
        actual.ShouldBe(expected, ignoreOrder, customMessage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Is<T>(
        [NotNullIfNotNull(nameof(expected))] this IEnumerable<T>? actual,
        [NotNullIfNotNull(nameof(actual))] IEnumerable<T>? expected,
        IEqualityComparer<T> comparer,
        bool ignoreOrder = false,
        string? customMessage = null)
    {
        actual.ShouldBe(expected, comparer, ignoreOrder, customMessage);
    }
}
