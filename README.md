# R3

The evolution of [dotnet/reactive](https://github.com/dotnet/reactive/) and [UniRx](https://github.com/neuecc/UniRx), which support many platforms including [Unity](https://unity.com/), [Godot](https://godotengine.org/), [Avalonia](https://avaloniaui.net/), WPF, etc(planning MAUI, [Stride](https://www.stride3d.net/), [LogicLooper](https://github.com/Cysharp/LogicLooper)).

> [!NOTE]
> This project is currently in preview. We are seeking a lot of feedback. We are considering fundamental changes such as [changing the name of the library (Uni(fied)Rx)](https://github.com/Cysharp/R3/issues/9) or [reverting back to the use of `IObservable<T>`](https://github.com/Cysharp/R3/issues/10) and others, if you have any opinions, please post them in the [Issues](https://github.com/Cysharp/R3/issues).

I have over 10 years of experience with Rx, experience in implementing a custom Rx runtime (UniRx) for game engine, and experience in implementing an asynchronous runtime ([UniTask](https://github.com/Cysharp/UniTask/)) for game engine. Based on those experiences, I came to believe that there is a need to implement a new Reactive Extensions for .NET, one that reflects modern C# and returns to the core values of Rx.

* Stopping the pipeline at OnError is a billion-dollar mistake.
* IScheduler is the root of poor performance.
* Frame-based operations, a missing feature in Rx, are especially important in game engines.
* Single asynchronous operations should be entirely left to async/await.
* Synchronous APIs should not be implemented.
* The Necessity of a subscription list to prevent subscription leaks (similar to a Parallel Debugger)
* Backpressure should be left to [IAsyncEnumerable](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream) and [Channels](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/).
* For distributed processing and queries, there are [GraphQL](https://graphql.org/), [Kubernetes](https://kubernetes.io/), [Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/), [Akka.NET](https://getakka.net/), [gRPC](https://grpc.io/), [MagicOnion](https://github.com/Cysharp/MagicOnion).

In other words, LINQ is not for EveryThing, and we believe that the essence of Rx lies in the processing of in-memory messaging (LINQ to Events), which will be our focus. Our main intended uses are UI frameworks and game engines, and we are not concerned with communication processes like [Reactive Streams](https://www.reactive-streams.org/).

To address the shortcomings of dotnet/reactive, we have made changes to the core interfaces. In recent years, Rx-like frameworks optimized for language features, such as [Kotlin Flow](https://kotlinlang.org/docs/flow.html) and [Swift Combine](https://developer.apple.com/documentation/combine), have been standardized. C# has also evolved significantly, now at C# 12, and we believe there is a need for an Rx that aligns with the latest C#.

> [!NOTE]
> TODO: mention about perfomrance and subscription tracker.

Getting Started
---
This library is distributed via NuGet, supporting .NET Standard 2.0, .NET Standard 2.1, .NET 6(.NET 7) and .NET 8 or above.

> PM> Install-Package [R3](https://www.nuget.org/packages/R3)

Some platforms(WPF, Avalonia, Unity, Godot) requires additional step to install. Please see [Platform Supports](#platform-supports) section in below.

R3 code is almostly same as standard Rx. Make the Observable via factory methods(Timer, Interval, FromEvent, Subject, etc...) and chain operator via LINQ methods. Therefore, your knowledge about Rx and documentation on Rx can be almost directly applied.

```csharp
using R3;

var subscription = Observable.Interval(TimeSpan.FromSeconds(1))
    .Select((_, i) => i)
    .Where(x => x % 2 == 0)
    .Subscribe(x => Console.WriteLine($"Interval:{x}"));

var cts = new CancellationTokenSource();
_ = Task.Run(() => { Console.ReadLine(); cts.Cancel(); });

await Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3))
    .TakeUntil(cts.Token)
    .ForEachAsync(x => Console.WriteLine($"Timer"));

subscription.Dispose();
```




















----




```csharp
public abstract class Observable<T>
{
    public IDisposable Subscribe(Observer<T> observer);
}

public abstract class Observer<T> : IDisposable
{
    public void OnNext(T value);
    public void OnErrorResume(Exception error);
    public void OnCompleted(Result result);
}

public static Observable<Unit> Interval(TimeSpan period, TimeProvider timeProvider);
public static Observable<Unit> IntervalFrame(int periodFrame, FrameProvider frameProvider);
```



`IObservable<T>` being the dual of `IEnumerable<T>` is a beautiful definition, but it was not very practical in use.





TimeProvider instead of IScheduler
---

Frame based operations
---


Disposables
---


Subscription Management
---


Platform Supports
---


### WPF


### Avalonia




### Unity

lower supported version: Unity 2021.3





### Godot





Operator Reference
---

### Factory

| Name(Parameter) | ReturnType |
| --- | --- |
| **Amb**(params `Observable<T>[]` sources) | `Observable<T>` |
| **Amb**(`IEnumerable<Observable<T>>` sources) | `Observable<T>` |
| **CombineLatest**(params `Observable<T>[]` sources) | `Observable<T[]>` |
| **CombineLatest**(`IEnumerable<Observable<T>>` sources) | `Observable<T[]>` |
| **Concat**(params `Observable<T>[]` sources) | `Observable<T>` |
| **Concat**(`IEnumerable<Observable<T>>` sources) | `Observable<T>` |
| **Create**(`Func<Observer<T>, IDisposable>` subscribe) | `Observable<T>` |
| **Create**(`TState` state, `Func<Observer<T>, TState, IDisposable>` subscribe) | `Observable<T>` |
| **Defer**(`Func<Observable<T>>` observableFactory) | `Observable<T>` |
| **Empty**() | `Observable<T>` |
| **Empty**(`TimeProvider` timeProvider) | `Observable<T>` |
| **Empty**(`TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` |
| **EveryUpdate**() | `Observable<Unit>` |
| **EveryUpdate**(`CancellationToken` cancellationToken) | `Observable<Unit>` |
| **EveryUpdate**(`FrameProvider` frameProvider) | `Observable<Unit>` |
| **EveryUpdate**(`FrameProvider` frameProvider, `CancellationToken` cancellationToken) | `Observable<Unit>` |
| **EveryValueChanged**(`TSource` source, `Func<TSource, TProperty>` propertySelector, `CancellationToken` cancellationToken = default) | `Observable<TProperty>` |
| **EveryValueChanged**(`TSource` source, `Func<TSource, TProperty>` propertySelector, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<TProperty>` |
| **EveryValueChanged**(`TSource` source, `Func<TSource, TProperty>` propertySelector, `EqualityComparer<TProperty>` equalityComparer, `CancellationToken` cancellationToken = default) | `Observable<TProperty>` |
| **EveryValueChanged**(`TSource` source, `Func<TSource, TProperty>` propertySelector, `FrameProvider` frameProvider, `EqualityComparer<TProperty>` equalityComparer, `CancellationToken` cancellationToken = default) | `Observable<TProperty>` |
| **FromEvent**(`Action<Action>` addHandler, `Action<Action>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **FromEvent**(`Action<Action<T>>` addHandler, `Action<Action<T>>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **FromEvent**(`Func<Action, TDelegate>` conversion, `Action<TDelegate>` addHandler, `Action<TDelegate>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **FromEvent**(`Func<Action<T>, TDelegate>` conversion, `Action<TDelegate>` addHandler, `Action<TDelegate>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **FromEventHandler**(`Action<EventHandler>` addHandler, `Action<EventHandler>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<ValueTuple<Object, EventArgs>>` |
| **FromEventHandler**(`Action<EventHandler<TEventArgs>>` addHandler, `Action<EventHandler<TEventArgs>>` removeHandler, `CancellationToken` cancellationToken = default) | `Observable<ValueTuple<Object, TEventArgs>>` |
| **Interval**(`TimeSpan` period, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Interval**(`TimeSpan` period, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **IntervalFrame**(`Int32` periodFrame, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **IntervalFrame**(`Int32` periodFrame, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Merge**(params `Observable<T>[]` sources) | `Observable<T>` |
| **Merge**(`IEnumerable<Observable<T>>` sources) | `Observable<T>` |
| **Never**() | `Observable<T>` |
| **NextFrame**(`CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **NextFrame**(`FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Range**(`Int32` start, `Int32` count) | `Observable<Int32>` |
| **Range**(`Int32` start, `Int32` count, `CancellationToken` cancellationToken) | `Observable<Int32>` |
| **Repeat**(`T` value, `Int32` count) | `Observable<T>` |
| **Repeat**(`T` value, `Int32` count, `CancellationToken` cancellationToken) | `Observable<T>` |
| **Return**(`T` value) | `Observable<T>` |
| **Return**(`T` value, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **Return**(`T` value, `TimeSpan` dueTime, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **Return**(`Unit` value) | `Observable<Unit>` |
| **Return**(`Boolean` value) | `Observable<Boolean>` |
| **Return**(`Int32` value) | `Observable<Int32>` |
| **ReturnFrame**(`T` value, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **ReturnFrame**(`T` value, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **ReturnFrame**(`T` value, `Int32` dueTimeFrame, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **ReturnFrame**(`T` value, `Int32` dueTimeFrame, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **ReturnOnCompleted**(`Result` result) | `Observable<T>` |
| **ReturnOnCompleted**(`Result` result, `TimeProvider` timeProvider) | `Observable<T>` |
| **ReturnOnCompleted**(`Result` result, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` |
| **ReturnUnit**() | `Observable<Unit>` |
| **Throw**(`Exception` exception) | `Observable<T>` |
| **Throw**(`Exception` exception, `TimeProvider` timeProvider) | `Observable<T>` |
| **Throw**(`Exception` exception, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` |
| **Timer**(`TimeSpan` dueTime, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`DateTimeOffset` dueTime, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`TimeSpan` dueTime, `TimeSpan` period, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`DateTimeOffset` dueTime, `TimeSpan` period, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`TimeSpan` dueTime, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`DateTimeOffset` dueTime, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`TimeSpan` dueTime, `TimeSpan` period, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Timer**(`DateTimeOffset` dueTime, `TimeSpan` period, `TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **TimerFrame**(`Int32` dueTimeFrame, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **TimerFrame**(`Int32` dueTimeFrame, `Int32` periodFrame, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **TimerFrame**(`Int32` dueTimeFrame, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **TimerFrame**(`Int32` dueTimeFrame, `Int32` periodFrame, `FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **ToObservable**(this `Task<T>` task) | `Observable<T>` |
| **ToObservable**(this `IEnumerable<T>` source, `CancellationToken` cancellationToken = default) | `Observable<T>` |
| **ToObservable**(this `IAsyncEnumerable<T>` source) | `Observable<T>` |
| **ToObservable**(this `IObservable<T>` source) | `Observable<T>` |
| **Yield**(`CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Yield**(`TimeProvider` timeProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **YieldFrame**(`CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **YieldFrame**(`FrameProvider` frameProvider, `CancellationToken` cancellationToken = default) | `Observable<Unit>` |
| **Zip**(params `Observable<T>[]` sources) | `Observable<T[]>` |
| **Zip**(`IEnumerable<Observable<T>>` sources) | `Observable<T[]>` |
| **ZipLatest**(params `Observable<T>[]` sources) | `Observable<T[]>` |
| **ZipLatest**(`IEnumerable<Observable<T>>` sources) | `Observable<T[]>` |

### Operator

| Name(Parameter) | ReturnType |
| --- | --- |
| **AggregateAsync**(this `Observable<T>` source, `TAccumulate` seed, `Func<TAccumulate, T, TAccumulate>` func, `Func<TAccumulate, TResult>` resultSelector, `CancellationToken` cancellationToken = default) | `Task<TResult>` |
| **AllAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **Amb**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` |
| **AnyAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **AnyAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **Append**(this `Observable<T>` source, `T` value) | `Observable<T>` |
| **AsIObservable**(this `Observable<T>` source) | `IObservable<T>` |
| **AsObservable**(this `Observable<T>` source) | `Observable<T>` |
| **AsUnitObservable**(this `Observable<T>` source) | `Observable<Unit>` |
| **AverageAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Double>` |
| **CancelOnCompleted**(this `Observable<T>` source, `CancellationTokenSource` cancellationTokenSource) | `Observable<T>` |
| **Cast**(this `Observable<T>` source) | `Observable<TResult>` |
| **Catch**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` |
| **Catch**(this `Observable<T>` source, `Func<TException, Observable<T>>` errorHandler) | `Observable<T>` |
| **Chunk**(this `Observable<T>` source, `Int32` count) | `Observable<T[]>` |
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T[]>` |
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T[]>` |
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `Int32` count) | `Observable<T[]>` |
| **Chunk**(this `Observable<T>` source, `TimeSpan` timeSpan, `Int32` count, `TimeProvider` timeProvider) | `Observable<T[]>` |
| **Chunk**(this `Observable<TSource>` source, `Observable<TWindowBoundary>` windowBoundaries) | `Observable<TSource[]>` |
| **ChunkFrame**(this `Observable<T>` source) | `Observable<T[]>` |
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T[]>` |
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T[]>` |
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `Int32` count) | `Observable<T[]>` |
| **ChunkFrame**(this `Observable<T>` source, `Int32` frameCount, `Int32` count, `FrameProvider` frameProvider) | `Observable<T[]>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Func<T1, T2, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Func<T1, T2, T3, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Func<T1, T2, T3, T4, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Func<T1, T2, T3, T4, T5, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Func<T1, T2, T3, T4, T5, T6, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Func<T1, T2, T3, T4, T5, T6, T7, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>` resultSelector) | `Observable<TResult>` |
| **CombineLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Observable<T15>` source15, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>` resultSelector) | `Observable<TResult>` |
| **Concat**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` |
| **ContainsAsync**(this `Observable<T>` source, `T` value, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **ContainsAsync**(this `Observable<T>` source, `T` value, `IEqualityComparer<T>` equalityComparer, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **CountAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Int32>` |
| **Debounce**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` |
| **Debounce**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` |
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
| **IgnoreElements**(this `Observable<T>` source) | `Observable<T>` |
| **IgnoreElements**(this `Observable<T>` source, `Action<T>` doOnNext) | `Observable<T>` |
| **IgnoreOnErrorResume**(this `Observable<T>` source) | `Observable<T>` |
| **IgnoreOnErrorResume**(this `Observable<T>` source, `Action<Exception>` doOnErrorResume) | `Observable<T>` |
| **IsEmptyAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Boolean>` |
| **LastAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **LastAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **LastOrDefaultAsync**(this `Observable<T>` source, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **LastOrDefaultAsync**(this `Observable<T>` source, `Func<T, Boolean>` predicate, `T` defaultValue = default, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **LongCountAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<Int64>` |
| **Materialize**(this `Observable<T>` source) | `Observable<Notification<T>>` |
| **MaxAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **MaxByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **MaxByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IComparer<TKey>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **Merge**(this `Observable<T>` source, `Observable<T>` second) | `Observable<T>` |
| **MinAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **MinByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **MinByAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IComparer<TKey>` comparer, `CancellationToken` cancellationToken = default) | `Task<T>` |
| **MinMaxAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<ValueTuple<T, T>>` |
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
| **Sample**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` |
| **Sample**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` |
| **SampleFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` |
| **SampleFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` |
| **Scan**(this `Observable<TSource>` source, `Func<TSource, TSource, TSource>` accumulator) | `Observable<TSource>` |
| **Scan**(this `Observable<TSource>` source, `TAccumulate` seed, `Func<TAccumulate, TSource, TAccumulate>` accumulator) | `Observable<TAccumulate>` |
| **Select**(this `Observable<T>` source, `Func<T, TResult>` selector) | `Observable<TResult>` |
| **Select**(this `Observable<T>` source, `Func<T, Int32, TResult>` selector) | `Observable<TResult>` |
| **Select**(this `Observable<T>` source, `TState` state, `Func<T, TState, TResult>` selector) | `Observable<TResult>` |
| **Select**(this `Observable<T>` source, `TState` state, `Func<T, Int32, TState, TResult>` selector) | `Observable<TResult>` |
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
| **SkipUntil**(this `Observable<T>` source, `Task` task) | `Observable<T>` |
| **SkipWhile**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` |
| **SkipWhile**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` |
| **SubscribeOn**(this `Observable<T>` source, `SynchronizationContext` synchronizationContext) | `Observable<T>` |
| **SubscribeOn**(this `Observable<T>` source, `TimeProvider` timeProvider) | `Observable<T>` |
| **SubscribeOn**(this `Observable<T>` source, `FrameProvider` frameProvider) | `Observable<T>` |
| **SubscribeOnCurrentSynchronizationContext**(this `Observable<T>` source) | `Observable<T>` |
| **SubscribeOnThreadPool**(this `Observable<T>` source) | `Observable<T>` |
| **SumAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T>` |
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
| **TakeUntil**(this `Observable<T>` source, `Task` task) | `Observable<T>` |
| **TakeWhile**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` |
| **TakeWhile**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` |
| **ThrottleFirst**(this `Observable<T>` source, `TimeSpan` timeSpan) | `Observable<T>` |
| **ThrottleFirst**(this `Observable<T>` source, `TimeSpan` timeSpan, `TimeProvider` timeProvider) | `Observable<T>` |
| **ThrottleFirstFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` |
| **ThrottleFirstFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` |
| **Timeout**(this `Observable<T>` source, `TimeSpan` dueTime) | `Observable<T>` |
| **Timeout**(this `Observable<T>` source, `TimeSpan` dueTime, `TimeProvider` timeProvider) | `Observable<T>` |
| **TimeoutFrame**(this `Observable<T>` source, `Int32` frameCount) | `Observable<T>` |
| **TimeoutFrame**(this `Observable<T>` source, `Int32` frameCount, `FrameProvider` frameProvider) | `Observable<T>` |
| **ToArrayAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<T[]>` |
| **ToAsyncEnumerable**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `IAsyncEnumerable<T>` |
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, T>>` |
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, T>>` |
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, TElement>>` |
| **ToDictionaryAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<Dictionary<TKey, TElement>>` |
| **ToHashSetAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<HashSet<T>>` |
| **ToHashSetAsync**(this `Observable<T>` source, `IEqualityComparer<T>` equalityComparer, `CancellationToken` cancellationToken = default) | `Task<HashSet<T>>` |
| **ToListAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task<List<T>>` |
| **ToLiveList**(this `Observable<T>` source) | `LiveList<T>` |
| **ToLiveList**(this `Observable<T>` source, `Int32` bufferSize) | `LiveList<T>` |
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, T>>` |
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, T>>` |
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, TElement>>` |
| **ToLookupAsync**(this `Observable<T>` source, `Func<T, TKey>` keySelector, `Func<T, TElement>` elementSelector, `IEqualityComparer<TKey>` keyComparer, `CancellationToken` cancellationToken = default) | `Task<ILookup<TKey, TElement>>` |
| **WaitAsync**(this `Observable<T>` source, `CancellationToken` cancellationToken = default) | `Task` |
| **Where**(this `Observable<T>` source, `Func<T, Boolean>` predicate) | `Observable<T>` |
| **Where**(this `Observable<T>` source, `Func<T, Int32, Boolean>` predicate) | `Observable<T>` |
| **Where**(this `Observable<T>` source, `TState` state, `Func<T, TState, Boolean>` predicate) | `Observable<T>` |
| **Where**(this `Observable<T>` source, `TState` state, `Func<T, Int32, TState, Boolean>` predicate) | `Observable<T>` |
| **WithLatestFrom**(this `Observable<TFirst>` first, `Observable<TSecond>` second, `Func<TFirst, TSecond, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Func<T1, T2, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Func<T1, T2, T3, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Func<T1, T2, T3, T4, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Func<T1, T2, T3, T4, T5, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Func<T1, T2, T3, T4, T5, T6, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Func<T1, T2, T3, T4, T5, T6, T7, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>` resultSelector) | `Observable<TResult>` |
| **Zip**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Observable<T15>` source15, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Func<T1, T2, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Func<T1, T2, T3, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Func<T1, T2, T3, T4, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Func<T1, T2, T3, T4, T5, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Func<T1, T2, T3, T4, T5, T6, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Func<T1, T2, T3, T4, T5, T6, T7, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>` resultSelector) | `Observable<TResult>` |
| **ZipLatest**(this `Observable<T1>` source1, `Observable<T2>` source2, `Observable<T3>` source3, `Observable<T4>` source4, `Observable<T5>` source5, `Observable<T6>` source6, `Observable<T7>` source7, `Observable<T8>` source8, `Observable<T9>` source9, `Observable<T10>` source10, `Observable<T11>` source11, `Observable<T12>` source12, `Observable<T13>` source13, `Observable<T14>` source14, `Observable<T15>` source15, `Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>` resultSelector) | `Observable<TResult>` |



Class/Method name changes from dotnet/reactive and UniRx
---
* `Buffer` -> `Chunk`
* `BatchFrame` -> `ChunkFrame`
* `Throttle` -> `Debounce`
* `ThrottleFrame` -> `DebounceFrame`
* `ObserveEveryValueChanged(this T value)` -> `Observable.EveryValueChanged(T value)`
* `Finally` -> `Do(onDisposed:)`
* `Do***` -> `Do(on***:)`
* `BehaviorSubject` -> `ReactiveProperty`
* `StableCompositeDisposable` -> `Disposable.Combine`
* `IScheduler` -> `TimeProvider`

License
---
This library is under the MIT License.