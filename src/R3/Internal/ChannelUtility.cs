using System.Threading.Channels;

namespace R3.Internal;

internal static class ChannelUtility
{
    static readonly UnboundedChannelOptions options = new UnboundedChannelOptions
    {
        SingleWriter = true, // in Rx operator, OnNext gurantees synchronous
        SingleReader = true, // almostly uses single reader loop
        AllowSynchronousContinuations = true // if false, uses TaskCreationOptions.RunContinuationsAsynchronously so avoid it.
    };

    internal static Channel<T> CreateSingleReadeWriterUnbounded<T>()
    {
        return Channel.CreateUnbounded<T>(options);
    }
}
