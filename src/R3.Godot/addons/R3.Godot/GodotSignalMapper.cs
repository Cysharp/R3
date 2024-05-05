using System;
using System.Threading;
using Godot;

namespace R3;

public static class GodotObjectExtensions
{
    public static Observable<Unit> SignalAsObservable(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<T> SignalAsObservable<[MustBeVariant] T>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7>(obj, signalName, completeOnExitTree).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7, T8)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(this GodotObject obj, StringName signalName, bool completeOnExitTree = true)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7, T8>(obj, signalName, completeOnExitTree).Observable;
    }
}

internal abstract class GodotSignalMapperBase<T> : IDisposable
{
    public Observable<T> Observable { get; private set; }

    protected Subject<T> subject = new();

    private readonly StringName godotSignalName;
    private GodotObject godotObject;
    private Callable? godotCallableDispose;
    private Callable? godotCallableOnNext;
    private int observerCount;
    private int disposed;

    protected GodotSignalMapperBase(GodotObject obj, StringName signalName, bool completeOnExitTree)
    {
        godotObject = obj;
        godotSignalName = signalName;
        Observable = subject.Do(
            this,
            onSubscribe: (mapper) => mapper.GainObserver(),
            onDispose: (mapper) => mapper.LoseObserver()
        );

        if (godotObject is Node node && completeOnExitTree)
        {
            if (godotSignalName != Node.SignalName.TreeExited)
            {
                godotCallableDispose = Callable.From(Dispose);
                node.Connect(Node.SignalName.TreeExited, godotCallableDispose.Value);
            }
            else
            {
                Observable = Observable.Take(1);
            }
        }
    }

    private void GainObserver()
    {
        lock (godotObject)
        {
            if (++observerCount > 1) { return; }

            if (!godotCallableOnNext.HasValue)
            {
                godotCallableOnNext = OnNextAsGodotCallable();
                godotObject.Connect(godotSignalName, godotCallableOnNext.Value);
            }
        }
    }

    private void LoseObserver()
    {
        lock (godotObject)
        {
            if (--observerCount > 0) { return; }

            if (godotCallableOnNext.HasValue)
            {
                godotObject.Disconnect(godotSignalName, godotCallableOnNext.Value);
                godotCallableOnNext = null;
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1) { return; }

        if (godotCallableDispose.HasValue) // can fail if completeOnExitTree was false
        {
            (godotObject as Node)?.Disconnect(Node.SignalName.TreeExited, godotCallableDispose.Value);
            godotCallableDispose = null;
        }
        subject.Dispose(true);
        Observable = R3.Observable.Empty<T>();
        subject = null;
        godotObject = null;
    }

    protected abstract Callable OnNextAsGodotCallable();
}

internal class GodotSignalMapper(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<Unit>(obj, signalName, completeOnExitTree) {
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action) OnNextWithUnit);
    private void OnNextWithUnit() => subject.OnNext(Unit.Default);
}

internal class GodotSignalMapper<[MustBeVariant] T>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<T>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T>) OnNextWithArgs);
    private void OnNextWithArgs(T _a) => subject.OnNext(_a);
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1) => subject.OnNext((_0, _1));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2) => subject.OnNext((_0, _1, _2));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3) => subject.OnNext((_0, _1, _2, _3));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3, T4)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4) => subject.OnNext((_0, _1, _2, _3, _4));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5) => subject.OnNext((_0, _1, _2, _3, _4, _5));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(GodotObject obj, StringName signalName, bool completeOnExitTree) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7, T8)>(obj, signalName, completeOnExitTree)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7, _8));
}
