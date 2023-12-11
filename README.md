# R3

Third Generation of Reactive Extensions.

```csharp
public abstract class Event<TMessage>
{
    public IDisposable Subscribe(Subscriber<TMessage> subscriber);
}

public abstract class Subscriber<TMessage> : IDisposable
{
    public void OnNext(TMessage message);
    public void OnErrorResume(Exception error);
}

// Completable
public abstract class CompletableEvent<TMessage, TComplete>
{
    public IDisposable Subscribe(Subscriber<TMessage, TComplete> subscriber)
}

public abstract class Subscriber<TMessage, TComplete> : IDisposable
{
    public void OnNext(TMessage message);
    public void OnErrorResume(Exception error);
    public void OnCompleted(TComplete complete);
}
```

```csharp
// similar as IObserver<T>
CompletableEvent<TMessage, Result<TComplete>>
```

