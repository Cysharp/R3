namespace R2;

// TODO:...
internal class OnErrorBubbling<TMessage>(IEvent<TMessage> source, Func<Exception, bool> onError) : IEvent<TMessage>
{
    public IDisposable Subscribe(ISubscriber<TMessage> subscriber)
    {
        return source.Subscribe(new _OnErrorBubbling(subscriber, onError));
    }

    class _OnErrorBubbling(ISubscriber<TMessage> subscriber, Func<Exception, bool> onError) : ISubscriber<TMessage>
    {
        public void OnNext(TMessage message)
        {
            try
            {
                subscriber.OnNext(message);
            }
            catch (Exception ex)
            {
                // true: stop propagation, false: re-throw
                if (!onError(ex))
                {
                    throw;
                }
            }
        }
    }
}
