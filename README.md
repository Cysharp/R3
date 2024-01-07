# R3

> [!NOTE]
> This project is currently in preview. We are seeking a lot of feedback. We are considering fundamental changes such as [changing the name of the library (Uni(fied)Rx)](https://github.com/Cysharp/R3/issues/9) or [reverting back to the use of `IObservable<T>`](https://github.com/Cysharp/R3/issues/10).

The evolution of dotnet/reactive and UniRx.


* LINQ is not for EveryThing

```csharp
public abstract class Observable<T>
{
    public IDisposable Subscribe(Observer<T> observer);
}

public abstract class Observer<T> : IDisposable
{
    public void OnNext(T value);
    public void OnErrorResume(Exception error);
    public void OnCompleted(Result result);
}
```



## Unity

lower supported version: Unity 2021.3

