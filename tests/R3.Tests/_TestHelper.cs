namespace R3.Tests;

public static class _TestHelper
{
    public static RecordList<T> LiveRecord<T>(this Event<T> source)
    {
        var l = new RecordList<T>();
        l.SourceSubscription.Disposable = source.Subscribe(x => l.Add(x));
        return l;
    }

    public static RecordList<T> LiveRecord<T, TC>(this CompletableEvent<T, TC> source)
    {
        var l = new RecordList<T>();
        l.SourceSubscription.Disposable = source.Subscribe(x => l.Add(x), _ => { l.IsCompleted = true; });
        return l;
    }
}

public sealed class RecordList<T> : List<T>, IDisposable
{
    public SingleAssignmentDisposableCore SourceSubscription;
    public bool IsCompleted { get; set; }

    public void Dispose()
    {
        SourceSubscription.Dispose();
    }
}
