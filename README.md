# R3

Third Generation of Reactive Extensions.

```csharp
public abstract class Event<T>
{
    public IDisposable Subscribe(Subscriber<T> subscriber);
}

public abstract class Subscriber<T> : IDisposable
{
    public void OnNext(T value);
    public void OnErrorResume(Exception error);
    public void OnCompleted(Result result);
}
```
