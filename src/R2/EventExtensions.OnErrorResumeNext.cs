namespace R2;

internal class OnErrorResumeNext<TMessage>(IEvent<TMessage> source, Action<Exception>? errorHandler) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _OnErrorResumeNext(subscriber, errorHandler));
    }

    class _OnErrorResumeNext(ISubscriber<TMessage> subscriber, Action<Exception>? errorHandler) : ISubscriber<TMessage>
    {
        public void OnNext(TMessage message)
        {
            try
            {
                subscriber.OnNext(message);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
            }
        }
    }
}


internal class OnErrorResumeNext2<TMessage>(IEvent<Result<TMessage>> source, Action<Exception>? errorHandler) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _OnErrorResumeNext(subscriber, errorHandler));
    }

    class _OnErrorResumeNext(ISubscriber<TMessage> subscriber, Action<Exception>? errorHandler) : ISubscriber<Result<TMessage>>
    {
        public void OnNext(Result<TMessage> message)
        {
            if (message.HasException)
            {
                errorHandler?.Invoke(message.Exception);
                return;
            }

            try
            {
                subscriber.OnNext(message.Value);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
            }
        }
    }
}


