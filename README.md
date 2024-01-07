# R3

> [!NOTE]
> This project is currently in preview. We are seeking a lot of feedback. We are considering fundamental changes such as [changing the name of the library (Uni(fied)Rx)](https://github.com/Cysharp/R3/issues/9) or [reverting back to the use of `IObservable<T>`](https://github.com/Cysharp/R3/issues/10) and others, if you have any opinions, please post them in the [Issues](https://github.com/Cysharp/R3/issues).

The evolution of [dotnet/reactive](https://github.com/dotnet/reactive/) and [UniRx](https://github.com/neuecc/UniRx), which support many platforms including Unity, Godot, Avalonia, WPF, etc(planning MAUI, Stride, LogicLooper).

I have over 10 years of experience with Rx, experience in implementing a custom Rx runtime (UniRx) for game engine, and experience in implementing an asynchronous runtime ([UniTask](https://github.com/Cysharp/UniTask/)) for game engine. Based on those experiences, I came to believe that there is a need to implement a new Reactive Extensions for .NET, one that reflects modern C# and returns to the core values of Rx.

* Stopping the pipeline at OnError is a billion-dollar mistake.
* IScheduler is the root of poor performance.
* Frame-based operations, a missing feature in Rx, are especially important in game engines.
* Single asynchronous operations should be entirely left to async/await.
* Synchronous APIs should not be implemented.
* The Necessity of a subscription list to prevent subscription leaks (similar to a Parallel Debugger)
* Backpressure should be left to [IAsyncEnumerable](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream) and [Channels](https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/).
* For distributed processing and transparent queries, there are [GraphQL](https://graphql.org/), [Kubernetes](https://kubernetes.io/), [Orleans](https://learn.microsoft.com/en-us/dotnet/orleans/), [gRPC](https://grpc.io/), and [MagicOnion](https://github.com/Cysharp/MagicOnion).

In other words, LINQ is not for EveryThing, and we believe that the essence of Rx lies in the processing of in-memory messaging (LINQ to Events), which will be our focus. Our main intended uses are UI frameworks and game engines, and we are not concerned with communication processes like [Reactive Streams](https://www.reactive-streams.org/).

To address the shortcomings of dotnet/reactive, we have made changes to the core interfaces, so R3 might appear to be an imitation of Rx. However, in recent years, Rx-like frameworks optimized for language features, such as [Kotlin Flow](https://kotlinlang.org/docs/flow.html) and [Swift Combine](https://developer.apple.com/documentation/combine), have been standardized. C# has also evolved significantly, now at C# 12, and we believe there is a need for an Rx that aligns with the latest C#.

Getting Started
---
















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



License
---
This library is under the MIT License.