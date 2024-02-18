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

    static readonly BoundedChannelOptions singularBoundedOptions = new BoundedChannelOptions(1)
    {
        SingleWriter = true, // in Rx operator, OnNext gurantees synchronous
        SingleReader = true, // almostly uses single reader loop
        AllowSynchronousContinuations = true, // if false, uses TaskCreationOptions.RunContinuationsAsynchronously so avoid it.
        FullMode = BoundedChannelFullMode.DropOldest, // This will ensure that the latest item to come in is always added
    };

    internal static Channel<T> CreateSingleReadeWriterUnbounded<T>()
    {
        return Channel.CreateUnbounded<T>(options);
    }

    internal static Channel<T> CreateSingleReadeWriterSingularBounded<T>()
    {
        return Channel.CreateBounded<T>(singularBoundedOptions);
    }
}
