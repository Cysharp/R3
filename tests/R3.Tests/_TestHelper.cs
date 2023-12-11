using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace R3.Tests;

public static class _TestHelper
{
    public static RecordList<T, Unit> LiveRecord<T>(this Event<T> source)
    {
        var l = new RecordList<T, Unit>();
        l.SourceSubscription.Disposable = source.Subscribe(x => l.Add(x));
        return l;
    }

    public static RecordList<T, TC> LiveRecord<T, TC>(this CompletableEvent<T, TC> source)
    {
        var l = new RecordList<T, TC>();
        l.SourceSubscription.Disposable = source.Subscribe(x => l.Add(x), x => { l.IsCompleted = true; l.CompletedValue = x; });
        return l;
    }
}

public sealed class RecordList<T, TC> : List<T>, IDisposable
{
    public SingleAssignmentDisposableCore SourceSubscription;
    public bool IsCompleted { get; set; }
    public TC? CompletedValue { get; set; }

    public void Dispose()
    {
        SourceSubscription.Dispose();
    }

    // test helper

    [MemberNotNull(nameof(CompletedValue))]
    public void AssertIsCompleted()
    {
        Debug.Assert(CompletedValue != null);
        IsCompleted.Should().BeTrue();
    }

    public void AssertIsNotCompleted()
    {
        IsCompleted.Should().BeFalse();
    }

    public void AssertCompletedValue(TC value)
    {
        CompletedValue.Should().Be(value);
    }

    public void AssertEqual(params T[] expected)
    {
        this.Should().Equal(expected);
    }
}
