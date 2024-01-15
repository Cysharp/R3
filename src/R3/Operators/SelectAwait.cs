using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static Observable<TResult> SelectAwait<T, TResult>(this Observable<T> source, Func<T, ValueTask<TResult>> selector, CancellationToken cancellationToken = default)
    {
        return new SelectAwait<T, TResult>(source, selector, cancellationToken);
    }
}

internal sealed class SelectAwait<T, TResult>(Observable<T> source, Func<T, ValueTask<TResult>> selector, CancellationToken cancellationToken) : Observable<TResult>
{
    protected override IDisposable SubscribeCore(Observer<TResult> observer)
    {
        // TODO: CreateLinked CancellationToken
        // var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tokenSource = new CancellationTokenSource();
        return source.Subscribe(new _SelectAwait(observer, selector, tokenSource));
    }

    sealed class _SelectAwait : Observer<T>
    {
        readonly Observer<TResult> observer;
        readonly Func<T, ValueTask<TResult>> selector;
        readonly CancellationTokenSource cancellationTokenSource;
        readonly Channel<T> channel;

        public _SelectAwait(Observer<TResult> observer, Func<T, ValueTask<TResult>> selector, CancellationTokenSource cancellationTokenSource)
        {
            this.observer = observer;
            this.selector = selector;
            this.cancellationTokenSource = cancellationTokenSource;
            this.channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

            RunQueueWorker(); // start reader loop
        }

        protected override void OnNextCore(T value)
        {
            channel.Writer.TryWrite(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            observer.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            observer.OnCompleted(result);
        }

        protected override void DisposeCore()
        {
            channel.Writer.Complete(); // complete writing
            cancellationTokenSource.Cancel(); // stop queue worker.
        }

        async void RunQueueWorker() // don't(can't) wait so use async void
        {
            var reader = channel.Reader;
            var token = cancellationTokenSource.Token;

            try
            {
                while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
                {
                    while (reader.TryRead(out var item))
                    {
                        try
                        {
                            var value = await selector(item); // use sync-context?
                            observer.OnNext(value);
                        }
                        catch (Exception ex)
                        {
                            observer.OnErrorResume(ex);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
            }
        }
    }
}
