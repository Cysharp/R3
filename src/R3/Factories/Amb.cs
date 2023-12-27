namespace R3;

public static partial class Observable
{
    public static IObservable<T> Amb<T>(params IObservable<T>[] sources)
    {
        throw new NotImplementedException();
    }

    public static IObservable<T> Amb<T>(IEnumerable<IObservable<T>> sources)
    {
        throw new NotImplementedException();
    }
}

internal sealed class Amb<T>(IEnumerable<IObservable<T>> sources) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        //new CompositeDiposableBuilder
        // Disposable.CreateBuilder();
        throw new NotImplementedException();
        //if (sources.TryGetNonEnumeratedCount(out var count))
        //{
            

        //}
        //else
        //{

        //}
        // throw new NotImplementedException();
    }


}
