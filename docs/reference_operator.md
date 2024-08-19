| Name(Parameter) | ReturnType | 
| --- | --- | 
| **AggregateAsync**(this `Observable<T>` source, `Func<T, T, T>` func, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **AggregateAsync**(this `Observable<T>` source, `TResult` seed, `Func<TResult, T, TResult>` func, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **AggregateAsync**(this `Observable<T>` source, `TAccumulate` seed, `Func<TAccumulate, T, TAccumulate>` func, `Func<TAccumulate, TResult>` resultSelector, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **AggregateByAsync**(this `Observable<TSource>` source, `Func<TSource, TKey>` keySelector, `TAccumulate` seed, `Func<TAccumulate, TSource, TAccumulate>` func, `IEqualityComparer<TKey>` keyComparer = default, `CancellationToken` cancellationToken = default) | `Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>>` | 
| **AggregateByAsync**(this `Observable<TSource>` source, `Func<TSource, TKey>` keySelector, `Func<TKey, TAccumulate>` seedSelector, `Func<TAccumulate, TSource, TAccumulate>` func, `IEqualityComparer<TKey>` keyComparer = default, `CancellationToken` cancellationToken = default) | `Task<IEnumerable<KeyValuePair<TKey, TAccumulate>>>` | 
| **AllAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **Amb**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` | 
| **AnyAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **AnyAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **Append**(this `Observable<T>` source, `T` value) | `Observable<T>` | 
| **Append**(this `Observable<T>` source, `IEnumerable<T>` values) | `Observable<T>` | 
| **Append**(this `Observable<T>` source, `Func<T>` valueFactory) | `Observable<T>` | 
| **Append**(this `Observable<T>` source, `TState` state, `Func<TState, T>` valueFactory) | `Observable<T>` | 
| **AsObservable**(this `Observable<T>` source) | `Observable<T>` | 
| **AsSystemObservable**(this `Observable<T>` source) | `IObservable<T>` | 
| **AsUnitObservable**(this `Observable<T>` source) | `Observable<Unit>` | 
| **AverageAsync**(this `Observable<Int32>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `Func<T, Int32>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<Int64>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `Func<T, Int64>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<Single>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `Func<T, Single>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<Double>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `Func<T, Double>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<Decimal>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `Func<T, Decimal>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **AverageAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **Cast**(this `Observable<T>` source) | `Observable<TResult>` | 
| **Catch**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` | 
| **Catch**(this `Observable<T>` source, `Func<TException, Observable<T>>` errorHandler) | `Observable<T>` | 
| **Chunk**(this `Observable<T>` source, `Int32` count) | `Observable<T[]>` | 
| **Chunk**(this `Observable<T>` source, `Int32` count, `Int32` skip) | `Observable<T[]>` | 
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T[]>` | 
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T[]>` | 
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `Int32` count) | `Observable<T[]>` | 
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `Int32` count, `TimeProvider` timeProvider) | `Observable<T[]>` | 
| **Chunk**(this `Observable<TSource>` source, `Observable<TWindowBoundary>` windowBoundaries) | `Observable<TSource[]>` | 
| **Chunk**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` asyncWindow, `Boolean` configureAwait = true) | `Observable<T[]>` | 
| **ChunkFrame**(this `Observable<T>` source) | `Observable<T[]>` | 
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T[]>` | 
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T[]>` | 
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `Int32` count) | `Observable<T[]>` | 
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `Int32` count, `FrameProvider` frameProvider) | `Observable<T[]>` | 
| **Concat**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` | 
| **ContainsAsync**(this `Observable<T>` source, `T` value, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **ContainsAsync**(this `Observable<T>` source, `T` value, `IEqualityComparer<T>` equalityComparer, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **CountAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Int32>` | 
| **CountAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Int32>` | 
| **Debounce**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` | 
| **Debounce**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` | 
| **Debounce**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` throttleDurationSelector, `Boolean` configureAwait = true) | `Observable<T>` | 
| **DebounceFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **DebounceFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **DefaultIfEmpty**(this `Observable<T>` source) | `Observable<T>` | 
| **DefaultIfEmpty**(this `Observable<T>` source, `T` defaultValue) | `Observable<T>` | 
| **Delay**(this `Observable<T>` source, `TimeSpan` dueTime) | `Observable<T>` | 
| **Delay**(this `Observable<T>` source, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` | 
| **DelayFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **DelayFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **DelaySubscription**(this `Observable<T>` source, `TimeSpan` dueTime) | `Observable<T>` | 
| **DelaySubscription**(this `Observable<T>` source, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` | 
| **DelaySubscriptionFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **DelaySubscriptionFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **Dematerialize**(this `Observable<Notification<T>>` source) | `Observable<T>` | 
| **Distinct**(this `Observable<T>` source) | `Observable<T>` | 
| **Distinct**(this `Observable<T>` source, `IEqualityComparer<T>` comparer) | `Observable<T>` | 
| **DistinctBy**(this `Observable<TSource>` source, `Func<TSource, TKey>` keySelector) | `Observable<TSource>` | 
| **DistinctBy**(this `Observable<TSource>` source, `Func<TSource, TKey>` keySelector, `IEqualityComparer<TKey>` comparer) | `Observable<TSource>` | 
| **DistinctUntilChanged**(this `Observable<T>` source) | `Observable<T>` | 
| **DistinctUntilChanged**(this `Observable<T>` source, `IEqualityComparer<T>` comparer) | `Observable<T>` | 
| **DistinctUntilChangedBy**(this `Observable<T>` source, `Func<T, TKey>` keySelector) | `Observable<T>` | 
| **DistinctUntilChangedBy**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IEqualityComparer<TKey>` comparer) | `Observable<T>` | 
| **Do**(this `Observable<T>` source, `Action<T>` onNext = default, `Action<Exception>` onErrorResume = default, `Action<Result>` onCompleted = default, `Action` onDispose = default, `Action` onSubscribe = default) | `Observable<T>` | 
| **Do**(this `Observable<T>` source, `TState` state, `Action<T, TState>` onNext = default, `Action<Exception, TState>` onErrorResume = default, `Action<Result, TState>` onCompleted = default, `Action<TState>` onDispose = default, `Action<TState>` onSubscribe = default) | `Observable<T>` | 
| **DoCancelOnCompleted**(this `Observable<T>` source, `CancellationTokenSource` cancellationTokenSource) | `Observable<T>` | 
| **ElementAtAsync**(this `Observable<T>` source, `Int32` index, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **ElementAtAsync**(this `Observable<T>` source, `Index` index, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **ElementAtOrDefaultAsync**(this `Observable<T>` source, `Int32` index, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **ElementAtOrDefaultAsync**(this `Observable<T>` source, `Index` index, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **FirstAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **FirstAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **FirstOrDefaultAsync**(this `Observable<T>` source, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **FirstOrDefaultAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **ForEachAsync**(this `Observable<T>` source, `Action<T>` action, `CancellationToken` cancellationToken = default) | `Task` | 
| **ForEachAsync**(this `Observable<T>` source, `Action<T, Int32>` action, `CancellationToken` cancellationToken = default) | `Task` | 
| **FrameCount**(this `Observable<T>` source) | `Observable<ValueTuple<Int64, T>>` | 
| **FrameCount**(this `Observable<T>` source, `FrameProvider` frameProvider) | `Observable<ValueTuple<Int64, T>>` | 
| **FrameInterval**(this `Observable<T>` source) | `Observable<ValueTuple<Int64, T>>` | 
| **FrameInterval**(this `Observable<T>` source, `FrameProvider` frameProvider) | `Observable<ValueTuple<Int64, T>>` | 
| **IgnoreElements**(this `Observable<T>` source) | `Observable<T>` | 
| **IgnoreElements**(this `Observable<T>` source, `Action<T>` doOnNext) | `Observable<T>` | 
| **IgnoreOnErrorResume**(this `Observable<T>` source) | `Observable<T>` | 
| **IgnoreOnErrorResume**(this `Observable<T>` source, `Action<Exception>` doOnErrorResume) | `Observable<T>` | 
| **Index**(this `Observable<Unit>` source) | `Observable<Int32>` | 
| **Index**(this `Observable<T>` source) | `Observable<ValueTuple<Int32, T>>` | 
| **IsEmptyAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **LastAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **LastAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **LastOrDefaultAsync**(this `Observable<T>` source, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **LastOrDefaultAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **LongCountAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Int64>` | 
| **LongCountAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Int64>` | 
| **Materialize**(this `Observable<T>` source) | `Observable<Notification<T>>` | 
| **MaxAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MaxAsync**(this `Observable<T>` source, `IComparer<T>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MaxAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **MaxAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `IComparer<TResult>` comparer, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **MaxByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MaxByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IComparer<TKey>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **Merge**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` | 
| **MinAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MinAsync**(this `Observable<T>` source, `IComparer<T>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MinAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **MinAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `IComparer<TResult>` comparer, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **MinByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MinByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IComparer<TKey>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **MinMaxAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<ValueTuple<T, T>>` | 
| **MinMaxAsync**(this `Observable<T>` source, `IComparer<T>` comparer, `CancellationToken` cancellationToken = default) | `Task<ValueTuple<T, T>>` | 
| **MinMaxAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `CancellationToken` cancellationToken = default) | `Task<ValueTuple<TResult, TResult>>` | 
| **MinMaxAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `IComparer<TResult>` comparer, `CancellationToken` cancellationToken = default) | `Task<ValueTuple<TResult, TResult>>` | 
| **Multicast**(this `Observable<T>` source, `ISubject<T>` subject) | `ConnectableObservable<T>` | 
| **ObserveOn**(this `Observable<T>` source, `SynchronizationContext` synchronizationContext) | `Observable<T>` | 
| **ObserveOn**(this `Observable<T>` source, `TimeProvider` timeProvider) | `Observable<T>` | 
| **ObserveOn**(this `Observable<T>` source, `FrameProvider` frameProvider) | `Observable<T>` | 
| **ObserveOnCurrentSynchronizationContext**(this `Observable<T>` source) | `Observable<T>` | 
| **ObserveOnThreadPool**(this `Observable<T>` source) | `Observable<T>` | 
| **OfType**(this `Observable<T>` source) | `Observable<TResult>` | 
| **OnErrorResumeAsFailure**(this `Observable<T>` source) | `Observable<T>` | 
| **Pairwise**(this `Observable<T>` source) | `Observable<ValueTuple<T, T>>` | 
| **Prepend**(this `Observable<T>` source, `T` value) | `Observable<T>` | 
| **Prepend**(this `Observable<T>` source, `IEnumerable<T>` values) | `Observable<T>` | 
| **Prepend**(this `Observable<T>` source, `Func<T>` valueFactory) | `Observable<T>` | 
| **Prepend**(this `Observable<T>` source, `TState` state, `Func<TState, T>` valueFactory) | `Observable<T>` | 
| **Publish**(this `Observable<T>` source) | `ConnectableObservable<T>` | 
| **Publish**(this `Observable<T>` source, `T` initialValue) | `ConnectableObservable<T>` | 
| **RefCount**(this `ConnectableObservable<T>` source) | `Observable<T>` | 
| **Replay**(this `Observable<T>` source) | `ConnectableObservable<T>` | 
| **Replay**(this `Observable<T>` source, `Int32` bufferSize) | `ConnectableObservable<T>` | 
| **Replay**(this `Observable<T>` source, `TimeSpan` window) | `ConnectableObservable<T>` | 
| **Replay**(this `Observable<T>` source, `TimeSpan` window, `TimeProvider` timeProvider) | `ConnectableObservable<T>` | 
| **Replay**(this `Observable<T>` source, `Int32` bufferSize, `TimeSpan` window) | `ConnectableObservable<T>` | 
| **Replay**(this `Observable<T>` source, `Int32` bufferSize, `TimeSpan` window, `TimeProvider` timeProvider) | `ConnectableObservable<T>` | 
| **ReplayFrame**(this `Observable<T>` source, `Int32` window) | `ConnectableObservable<T>` | 
| **ReplayFrame**(this `Observable<T>` source, `Int32` window, `FrameProvider` frameProvider) | `ConnectableObservable<T>` | 
| **ReplayFrame**(this `Observable<T>` source, `Int32` bufferSize, `Int32` window) | `ConnectableObservable<T>` | 
| **ReplayFrame**(this `Observable<T>` source, `Int32` bufferSize, `Int32` window, `FrameProvider` frameProvider) | `ConnectableObservable<T>` | 
| **Scan**(this `Observable<TSource>` source, `Func<TSource, TSource, TSource>` accumulator) | `Observable<TSource>` | 
| **Scan**(this `Observable<TSource>` source, `TAccumulate` seed, `Func<TAccumulate, TSource, TAccumulate>` accumulator) | `Observable<TAccumulate>` | 
| **Select**(this `Observable<T>` source, `Func<T, TResult>` selector) | `Observable<TResult>` | 
| **Select**(this `Observable<T>` source, `Func<T, Int32, TResult>` selector) | `Observable<TResult>` | 
| **Select**(this `Observable<T>` source, `TState` state, `Func<T, TState, TResult>` selector) | `Observable<TResult>` | 
| **Select**(this `Observable<T>` source, `TState` state, `Func<T, Int32, TState, TResult>` selector) | `Observable<TResult>` | 
| **SelectAwait**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask<TResult>>` selector, `AwaitOperation` awaitOperation = AwaitOperation.Sequential, `Boolean` configureAwait = true, `Boolean` cancelOnCompleted = false, `Int32` maxConcurrent = -1) | `Observable<TResult>` | 
| **SelectMany**(this `Observable<TSource>` source, `Func<TSource, Observable<TResult>>` selector) | `Observable<TResult>` | 
| **SelectMany**(this `Observable<TSource>` source, `Func<TSource, Observable<TCollection>>` collectionSelector, `Func<TSource, TCollection, TResult>` resultSelector) | `Observable<TResult>` | 
| **SelectMany**(this `Observable<TSource>` source, `Func<TSource, Int32, Observable<TResult>>` selector) | `Observable<TResult>` | 
| **SelectMany**(this `Observable<TSource>` source, `Func<TSource, Int32, Observable<TCollection>>` collectionSelector, `Func<TSource, Int32, TCollection, Int32, TResult>` resultSelector) | `Observable<TResult>` | 
| **SequenceEqualAsync**(this `Observable<T>` source, `Observable<T>` second, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **SequenceEqualAsync**(this `Observable<T>` source, `Observable<T>` second, `IEqualityComparer<T>` equalityComparer, `CancellationToken` cancellationToken = default) | `Task<Boolean>` | 
| **Share**(this `Observable<T>` source) | `Observable<T>` | 
| **SingleAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **SingleAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **SingleOrDefaultAsync**(this `Observable<T>` source, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **SingleOrDefaultAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **Skip**(this `Observable<T>` source, `Int32` count) | `Observable<T>` | 
| **Skip**(this `Observable<T>` source, `TimeSpan` duration) | `Observable<T>` | 
| **Skip**(this `Observable<T>` source, `TimeSpan` duration, `TimeProvider` timeProvider) | `Observable<T>` | 
| **SkipFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **SkipFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **SkipLast**(this `Observable<T>` source, `Int32` count) | `Observable<T>` | 
| **SkipLast**(this `Observable<T>` source, `TimeSpan` duration) | `Observable<T>` | 
| **SkipLast**(this `Observable<T>` source, `TimeSpan` duration, `TimeProvider` timeProvider) | `Observable<T>` | 
| **SkipLastFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **SkipLastFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **SkipUntil**(this `Observable<T>` source, `Observable<TOther>` other) | `Observable<T>` | 
| **SkipUntil**(this `Observable<T>` source, `CancellationToken` cancellationToken) | `Observable<T>` | 
| **SkipUntil**(this `Observable<T>` source, `Task` task, `Boolean` configureAwait = true) | `Observable<T>` | 
| **SkipUntil**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` asyncFunc, `Boolean` configureAwait = true) | `Observable<T>` | 
| **SkipWhile**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` | 
| **SkipWhile**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` | 
| **SubscribeAwait**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` onNextAsync, `AwaitOperation` awaitOperation = AwaitOperation.Sequential, `Boolean` configureAwait = true, `Boolean` cancelOnCompleted = false, `Int32` maxConcurrent = -1) | `IDisposable` | 
| **SubscribeAwait**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` onNextAsync, `Action<Result>` onCompleted, `AwaitOperation` awaitOperation = AwaitOperation.Sequential, `Boolean` configureAwait = true, `Boolean` cancelOnCompleted = false, `Int32` maxConcurrent = -1) | `IDisposable` | 
| **SubscribeAwait**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` onNextAsync, `Action<Exception>` onErrorResume, `Action<Result>` onCompleted, `AwaitOperation` awaitOperation = AwaitOperation.Sequential, `Boolean` configureAwait = true, `Boolean` cancelOnCompleted = false, `Int32` maxConcurrent = -1) | `IDisposable` | 
| **SubscribeOn**(this `Observable<T>` source, `SynchronizationContext` synchronizationContext) | `Observable<T>` | 
| **SubscribeOn**(this `Observable<T>` source, `TimeProvider` timeProvider) | `Observable<T>` | 
| **SubscribeOn**(this `Observable<T>` source, `FrameProvider` frameProvider) | `Observable<T>` | 
| **SubscribeOnCurrentSynchronizationContext**(this `Observable<T>` source) | `Observable<T>` | 
| **SubscribeOnSynchronize**(this `Observable<T>` source, `Object` gate, `Boolean` rawObserver = false) | `Observable<T>` | 
| **SubscribeOnThreadPool**(this `Observable<T>` source) | `Observable<T>` | 
| **SumAsync**(this `Observable<Int32>` source, `CancellationToken` cancellationToken = default) | `Task<Int32>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, Int32>` selector, `CancellationToken` cancellationToken = default) | `Task<Int32>` | 
| **SumAsync**(this `Observable<Int64>` source, `CancellationToken` cancellationToken = default) | `Task<Int64>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, Int64>` selector, `CancellationToken` cancellationToken = default) | `Task<Int64>` | 
| **SumAsync**(this `Observable<Single>` source, `CancellationToken` cancellationToken = default) | `Task<Single>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, Single>` selector, `CancellationToken` cancellationToken = default) | `Task<Single>` | 
| **SumAsync**(this `Observable<Double>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, Double>` selector, `CancellationToken` cancellationToken = default) | `Task<Double>` | 
| **SumAsync**(this `Observable<Decimal>` source, `CancellationToken` cancellationToken = default) | `Task<Decimal>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, Decimal>` selector, `CancellationToken` cancellationToken = default) | `Task<Decimal>` | 
| **SumAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` | 
| **SumAsync**(this `Observable<TSource>` source, `Func<TSource, TResult>` selector, `CancellationToken` cancellationToken = default) | `Task<TResult>` | 
| **Switch**(this `Observable<Observable<T>>` sources) | `Observable<T>` | 
| **Synchronize**(this `Observable<T>` source) | `Observable<T>` | 
| **Synchronize**(this `Observable<T>` source, `Object` gate) | `Observable<T>` | 
| **Take**(this `Observable<T>` source, `Int32` count) | `Observable<T>` | 
| **Take**(this `Observable<T>` source, `TimeSpan` duration) | `Observable<T>` | 
| **Take**(this `Observable<T>` source, `TimeSpan` duration, `TimeProvider` timeProvider) | `Observable<T>` | 
| **TakeFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **TakeFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **TakeLast**(this `Observable<T>` source, `Int32` count) | `Observable<T>` | 
| **TakeLast**(this `Observable<T>` source, `TimeSpan` duration) | `Observable<T>` | 
| **TakeLast**(this `Observable<T>` source, `TimeSpan` duration, `TimeProvider` timeProvider) | `Observable<T>` | 
| **TakeLastFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **TakeLastFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `Observable<TOther>` other) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `CancellationToken` cancellationToken) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `Task` task, `Boolean` configureAwait = true) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` asyncFunc, `Boolean` configureAwait = true) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` | 
| **TakeUntil**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` | 
| **TakeWhile**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` | 
| **TakeWhile**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` | 
| **ThrottleFirst**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` | 
| **ThrottleFirst**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` | 
| **ThrottleFirst**(this `Observable<T>` source, `Observable<TSample>` sampler) | `Observable<T>` | 
| **ThrottleFirst**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` sampler, `Boolean` configureAwait = true) | `Observable<T>` | 
| **ThrottleFirstFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **ThrottleFirstFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **ThrottleFirstLast**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` | 
| **ThrottleFirstLast**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` | 
| **ThrottleFirstLast**(this `Observable<T>` source, `Observable<TSample>` sampler) | `Observable<T>` | 
| **ThrottleFirstLast**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` sampler, `Boolean` configureAwait = true) | `Observable<T>` | 
| **ThrottleFirstLastFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **ThrottleFirstLastFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **ThrottleLast**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` | 
| **ThrottleLast**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` | 
| **ThrottleLast**(this `Observable<T>` source, `Observable<TSample>` sampler) | `Observable<T>` | 
| **ThrottleLast**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask>` sampler, `Boolean` configureAwait = true) | `Observable<T>` | 
| **ThrottleLastFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **ThrottleLastFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **TimeInterval**(this `Observable<T>` source) | `Observable<ValueTuple<TimeSpan, T>>` | 
| **TimeInterval**(this `Observable<T>` source, `TimeProvider` timeProvider) | `Observable<ValueTuple<TimeSpan, T>>` | 
| **Timeout**(this `Observable<T>` source, `TimeSpan` dueTime) | `Observable<T>` | 
| **Timeout**(this `Observable<T>` source, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` | 
| **TimeoutFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` | 
| **TimeoutFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` | 
| **Timestamp**(this `Observable<T>` source) | `Observable<ValueTuple<Int64, T>>` | 
| **Timestamp**(this `Observable<T>` source, `TimeProvider` timeProvider) | `Observable<ValueTuple<Int64, T>>` | 
| **ToArrayAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T[]>` | 
| **ToAsyncEnumerable**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `IAsyncEnumerable<T>` | 
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, T>>` | 
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, T>>` | 
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, TElement>>` | 
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, TElement>>` | 
| **ToHashSetAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<HashSet<T>>` | 
| **ToHashSetAsync**(this `Observable<T>` source, `IEqualityComparer<T>` comparer, `CancellationToken` cancellationToken = default) | `Task<HashSet<T>>` | 
| **ToListAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<List<T>>` | 
| **ToLiveList**(this `Observable<T>` source) | `LiveList<T>` | 
| **ToLiveList**(this `Observable<T>` source, `Int32` bufferSize) | `LiveList<T>` | 
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, T>>` | 
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, T>>` | 
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, TElement>>` | 
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, TElement>>` | 
| **Trampoline**(this `Observable<T>` source) | `Observable<T>` | 
| **WaitAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task` | 
| **Where**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` | 
| **Where**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` | 
| **Where**(this `Observable<T>` source, `TState` state, `Func<T, TState, Boolean>` predicate) | `Observable<T>` | 
| **Where**(this `Observable<T>` source, `TState` state, `Func<T, Int32, TState, Boolean>` predicate) | `Observable<T>` | 
| **WhereAwait**(this `Observable<T>` source, `Func<T, CancellationToken, ValueTask<Boolean>>` predicate, `AwaitOperation` awaitOperation = AwaitOperation.Sequential, `Boolean` configureAwait = true, `Boolean` cancelOnCompleted = false, `Int32` maxConcurrent = -1) | `Observable<T>` | 
| **WhereNotNull**(this `Observable<TResult>` source) | `Observable<TResult>` | 
| **WithLatestFrom**(this `Observable<TFirst>` first, `Observable<TSecond>` second, `Func<TFirst, TSecond, TResult>` resultSelector) | `Observable<TResult>` | 


