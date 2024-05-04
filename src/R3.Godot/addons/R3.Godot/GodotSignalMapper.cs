using System;
using Godot;

namespace R3;

public static class GodotObjectExtensions
{
    public static Observable<Unit> SignalAsObservable(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper(obj, signalName).Observable;
    }

    public static Observable<T> SignalAsObservable<[MustBeVariant] T>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7>(obj, signalName).Observable;
    }

    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7, T8)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(this GodotObject obj, StringName signalName)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7, T8>(obj, signalName).Observable;
    }
}

internal abstract class GodotSignalMapperBase<T>
{
    public readonly Observable<T> Observable;

    protected readonly Subject<T> subject = new();

    private Callable? godotCallable;
    private readonly GodotObject godotObject;
    private readonly StringName godotSignalName;
    private int observerCount;

    protected GodotSignalMapperBase(GodotObject obj, StringName signalName)
    {
        godotObject = obj;
        godotSignalName = signalName;
        Observable = subject.Do(
            this,
            onSubscribe: (mapper) => mapper.GainObserver(),
            onDispose: (mapper) => mapper.LoseObserver()
        );
    }

    private void GainObserver()
    {
        if (!godotCallable.HasValue)
        {
            var callable = OnNextAsGodotCallable();
            godotObject.Connect(godotSignalName, callable);
            godotCallable = callable;
        }
        ++observerCount;
    }

    private void LoseObserver()
    {
        if (--observerCount > 0) { return; }
        if (godotCallable.HasValue)
        {
            godotObject.Disconnect(godotSignalName, godotCallable.Value);
            godotCallable = null;
        }
    }

    protected abstract Callable OnNextAsGodotCallable();
}

internal class GodotSignalMapper(GodotObject obj, StringName signalName) : GodotSignalMapperBase<Unit>(obj, signalName) {
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action) OnNextWithUnit);
    private void OnNextWithUnit() => subject.OnNext(Unit.Default);
}

internal class GodotSignalMapper<[MustBeVariant] T>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<T>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T>) OnNextWithArgs);
    private void OnNextWithArgs(T _a) => subject.OnNext(_a);
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1) => subject.OnNext((_0, _1));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2) => subject.OnNext((_0, _1, _2));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3) => subject.OnNext((_0, _1, _2, _3));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3, T4)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4) => subject.OnNext((_0, _1, _2, _3, _4));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5) => subject.OnNext((_0, _1, _2, _3, _4, _5));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(GodotObject obj, StringName signalName) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7, T8)>(obj, signalName)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7, _8));
}
