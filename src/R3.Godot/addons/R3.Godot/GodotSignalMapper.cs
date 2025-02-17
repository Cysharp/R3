using System;
using System.Threading;
using Godot;

namespace R3;

public static class GodotObjectExtensions
{
    /// <summary>Returns a <see cref="CancellationToken"/> that cancels when <paramref name="obj"/> emits the specified signal, this cancellation is one-shot unless <paramref name="oneShot"/> is false.</summary>
    public static CancellationToken CancelOnSignal(this GodotObject obj, StringName signalName, bool oneShot = true)
    {
        CancellationTokenSource cts = new();
        obj.Connect(signalName, Callable.From(cts.Cancel), oneShot ? (uint) GodotObject.ConnectFlags.OneShot : 0);
        return cts.Token;
    }

    /// <summary>Returns an observable that: publishes a <see cref="Unit"/> when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<Unit> SignalAsObservable(this Node node, StringName signalName)
    {
        return node.SignalAsObservable(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes data of type <typeparamref name="T"/> when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<T> SignalAsObservable<[MustBeVariant] T>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3, T4)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3, T4>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3, T4, T5>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3, T4, T5, T6>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3, T4, T5, T6, T7>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>, <typeparamref name="T8"/>) when <paramref name="node"/> emits the specified signal (except for <see cref="Node.SignalName.TreeExited"/>); finishes when <paramref name="node"/> emits <see cref="Node.SignalName.TreeExited"/>.</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7, T8)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(this Node node, StringName signalName)
    {
        return node.SignalAsObservable<T0, T1, T2, T3, T4, T5, T6, T7, T8>(signalName, node.CancelOnSignal(Node.SignalName.TreeExited));
    }

    /// <summary>Returns an observable that: publishes a <see cref="Unit"/> when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<Unit> SignalAsObservable(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes data of type <typeparamref name="T"/> when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<T> SignalAsObservable<[MustBeVariant] T>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3, T4)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7>(obj, signalName, cancellationToken).RefCount();
    }

    /// <summary>Returns an observable that: publishes a tuple (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>, <typeparamref name="T7"/>, <typeparamref name="T8"/>) when <paramref name="obj"/> emits the specified signal; finishes when <paramref name="cancellationToken"/> cancels, or when all subscriptions are disposed, if only <paramref name="cancellationToken"/> is default (= <see cref="CancellationToken.None"/>).</summary>
    public static Observable<(T0, T1, T2, T3, T4, T5, T6, T7, T8)> SignalAsObservable<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(this GodotObject obj, StringName signalName, CancellationToken cancellationToken = default)
    {
        return new GodotSignalMapper<T0, T1, T2, T3, T4, T5, T6, T7, T8>(obj, signalName, cancellationToken).RefCount();
    }
}

internal abstract class GodotSignalMapperBase<T> : ConnectableObservable<T>, IDisposable
{
    protected readonly Subject<T> subject = new();

    private readonly GodotObject godotObject;
    private readonly StringName godotSignalName;
    private readonly CancellationTokenRegistration? cancellationTokenRegistration;
    private Callable? godotCallableOnNext;
    private int connected;
    private int disposed;

    private bool ShouldDisposeOnDisconnect => !cancellationTokenRegistration.HasValue;

    protected GodotSignalMapperBase(GodotObject obj, StringName signalName, CancellationToken cancellationToken)
    {
        godotObject = obj;
        godotSignalName = signalName;
        if (cancellationToken.CanBeCanceled)
        {
            cancellationTokenRegistration = cancellationToken.UnsafeRegister((state) => ((GodotSignalMapperBase<T>) state!).Dispose(), this);
        }
    }

    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return subject.Subscribe(observer.Wrap());
    }

    public override IDisposable Connect()
    {
        if (Interlocked.CompareExchange(ref connected, 1, 0) == 0)
        {
            godotCallableOnNext ??= OnNextAsGodotCallable();
            godotObject.Connect(godotSignalName, godotCallableOnNext.Value);
        }
        return new Connection(this);
    }

    private void Disconnect()
    {
        if (Interlocked.CompareExchange(ref connected, 0, 1) == 1)
        {
            godotObject.Disconnect(godotSignalName, godotCallableOnNext!.Value);
        }
    }

    private class Connection(GodotSignalMapperBase<T> parent) : IDisposable
    {
        public void Dispose()
        {
            parent.Disconnect();
            if (parent.ShouldDisposeOnDisconnect) { parent.Dispose(); }
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1) { return; }
        Disconnect();
        subject.Dispose(true);
        cancellationTokenRegistration?.Dispose();
    }

    protected abstract Callable OnNextAsGodotCallable();
}

internal class GodotSignalMapper(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<Unit>(obj, signalName, cancellationToken) {
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action) OnNextWithUnit);
    private void OnNextWithUnit() => subject.OnNext(Unit.Default);
}

internal class GodotSignalMapper<[MustBeVariant] T>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<T>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T>) OnNextWithArgs);
    private void OnNextWithArgs(T _a) => subject.OnNext(_a);
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1) => subject.OnNext((_0, _1));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2) => subject.OnNext((_0, _1, _2));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3) => subject.OnNext((_0, _1, _2, _3));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3, T4)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4) => subject.OnNext((_0, _1, _2, _3, _4));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5) => subject.OnNext((_0, _1, _2, _3, _4, _5));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7));
}

internal class GodotSignalMapper<[MustBeVariant] T0, [MustBeVariant] T1, [MustBeVariant] T2, [MustBeVariant] T3, [MustBeVariant] T4, [MustBeVariant] T5, [MustBeVariant] T6, [MustBeVariant] T7, [MustBeVariant] T8>(GodotObject obj, StringName signalName, CancellationToken cancellationToken) : GodotSignalMapperBase<(T0, T1, T2, T3, T4, T5, T6, T7, T8)>(obj, signalName, cancellationToken)
{
    protected override Callable OnNextAsGodotCallable() => Callable.From((Action<T0, T1, T2, T3, T4, T5, T6, T7, T8>) OnNextWithArgs);
    private void OnNextWithArgs(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8) => subject.OnNext((_0, _1, _2, _3, _4, _5, _6, _7, _8));
}
