namespace R3.Operators
{
    // TODO:...
    internal class OnErrorBubbling<TMessage>(Event<TMessage> source, Func<Exception, bool> onError) : Event<TMessage>
    {
        protected override IDisposable SubscribeCore(Subscriber<TMessage> subscriber)
        {
            return source.Subscribe(new _OnErrorBubbling(subscriber, onError));
        }

        class _OnErrorBubbling(Subscriber<TMessage> subscriber, Func<Exception, bool> onError) : Subscriber<TMessage>
        {
            public override void OnNextCore(TMessage message)
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
}
