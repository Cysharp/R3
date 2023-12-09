namespace R2;

internal class OnErrorResumeNext<TMessage>(Event<TMessage> source, Action<Exception>? errorHandler) : Event<TMessage>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _OnErrorResumeNext(subscriber, errorHandler));
    }

    sealed class _OnErrorResumeNext(Subscriber<TMessage> subscriber, Action<Exception>? errorHandler) : Subscriber<TMessage>
    {
        public override void OnNext(TMessage message)
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


internal class OnErrorResumeNext2<TMessage>(Event<Result<TMessage>> source, Action<Exception>? errorHandler) : Event<TMessage>
{
    protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _OnErrorResumeNext(subscriber, errorHandler));
    }

    sealed class _OnErrorResumeNext(Subscriber<TMessage> subscriber, Action<Exception>? errorHandler) : Subscriber<Result<TMessage>>
    {
        public override void OnNext(Result<TMessage> message)
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
