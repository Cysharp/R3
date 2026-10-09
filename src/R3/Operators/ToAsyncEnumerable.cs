#if !NETSTANDARD2_0

using System.Threading.Channels;

namespace R3;

public static partial class ObservableExtensions
{
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Observable<T> source, CancellationToken cancellationToken = default)
    {
        var channel = ChannelUtility.CreateSingleReadeWriterUnbounded<T>();

        var observer = new ToAsyncEnumerable<T>(channel.Writer);
        var disposable = source.Subscribe(observer);

        if (cancellationToken.CanBeCanceled)
        {
            observer.registration = cancellationToken.UnsafeRegister(state =>
            {
                ((IDisposable)state!).Dispose(); // cancel IAsyncEnumerable<T> may call from ReadAllAsync so don't care in here.
            }, disposable);
        }

        observer.readerEnumerable = channel.Reader.ReadAllAsync(cancellationToken);
        return observer;
    }
}

sealed class ToAsyncEnumerable<T>(ChannelWriter<T> writer) : Observer<T>, IAsyncEnumerable<T>
{
    public CancellationTokenRegistration registration;

    public IAsyncEnumerable<T> readerEnumerable = null!;

    protected override void OnNextCore(T value)
    {
        writer.TryWrite(value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        writer.TryComplete(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            writer.TryComplete(result.Exception);
        }
        else
        {
            writer.TryComplete();
        }
    }

    protected override void DisposeCore()
    {
        registration.Dispose();
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator_(this, readerEnumerable.GetAsyncEnumerator(cancellationToken));
    }

    sealed class AsyncEnumerator_(ToAsyncEnumerable<T> owner, IAsyncEnumerator<T> enumerator) : IAsyncEnumerator<T>
    {
        public T Current => enumerator.Current;
        public async ValueTask DisposeAsync()
        {
            await enumerator.DisposeAsync();
            owner.Dispose();
        }
        public ValueTask<bool> MoveNextAsync()
        {
            return enumerator.MoveNextAsync();
        }
    }
}

#endif
