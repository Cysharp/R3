namespace R3;

public static partial class Observable
{
    // Never
    public static Observable<T> Never<T>()
    {
        return R3.Never<T>.Instance;
    }
}

internal sealed class Never<T> : Observable<T>
{
    // singleton
    public static readonly Never<T> Instance = new Never<T>();

    Never()
    {

    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return Disposable.Empty;
    }
}
