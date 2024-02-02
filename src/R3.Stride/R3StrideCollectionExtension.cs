using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Collections;
using System.Threading;

namespace R3;
public static class R3StrideCollectionExtension
{
    public static Observable<(object? sender, TrackingCollectionChangedEventArgs arg)> CollectionChangedAsObservable(this ITrackingCollectionChanged hashset, CancellationToken token = default)
    {
        return Observable.FromEventHandler<TrackingCollectionChangedEventArgs>(h => hashset.CollectionChanged += h, h => hashset.CollectionChanged -= h, token);
    }
    public static Observable<(object? sender, FastTrackingCollectionChangedEventArgs arg)> CollectionChangedAsObservable<T>(this FastTrackingCollection<T> collection, CancellationToken token = default)
    {
        return Observable.FromEvent<FastTrackingCollection<T>.FastEventHandler<FastTrackingCollectionChangedEventArgs>, (object?, FastTrackingCollectionChangedEventArgs)>(
            h =>
            {
                void Handler(object? sender, ref FastTrackingCollectionChangedEventArgs arg)
                {
                    h((sender, arg));
                }
                return new FastTrackingCollection<T>.FastEventHandler<FastTrackingCollectionChangedEventArgs>(Handler);
            },
            h => collection.CollectionChanged += h,
            h => collection.CollectionChanged -= h,
            token);
    }
}
