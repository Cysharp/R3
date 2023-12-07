namespace R2;

public interface IEvent<out TMessage>
{
    IDisposable Subscribe(ISubscriber<TMessage> subscriber);
}

public interface ISubscriber<in TMessage>
{
    void OnNext(TMessage message);
}

public interface ICompletableEvent<out TMessage, out TComplete>
{
    IDisposable Subscribe(ISubscriber<TMessage, TComplete> subscriber);
}

public interface ISubscriber<in TMessage, in TComplete>
{
    void OnNext(TMessage message);
    void OnCompleted(TComplete complete);
}