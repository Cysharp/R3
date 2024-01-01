namespace R3;

public abstract class ConnectableObservable<T> : Observable<T>
{
    public abstract IDisposable Connect();
}
