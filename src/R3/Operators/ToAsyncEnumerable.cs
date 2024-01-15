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

        return channel.Reader.ReadAllAsync(cancellationToken);
    }
}

sealed class ToAsyncEnumerable<T>(ChannelWriter<T> writer) : Observer<T>
{
    public CancellationTokenRegistration registration;

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
}

#endif
