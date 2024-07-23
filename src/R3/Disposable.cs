using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace R3;

public static class Disposable
{
    public static readonly IDisposable Empty = new EmptyDisposable();

    public static DisposableBuilder CreateBuilder()
    {
        return new DisposableBuilder();
    }

    public static T AddTo<T>(this T disposable, ref DisposableBuilder builder)
        where T : IDisposable
    {
        builder.Add(disposable);
        return disposable;
    }

    public static T AddTo<T>(this T disposable, ref DisposableBag bag)
        where T : IDisposable
    {
        bag.Add(disposable);
        return disposable;
    }

    public static T AddTo<T>(this T disposable, ICollection<IDisposable> disposables)
        where T : IDisposable
    {
        disposables.Add(disposable);
        return disposable;
    }

    // AddTo is already used in UniTask so avoid name conflict...

    public static CancellationTokenRegistration RegisterTo(this IDisposable disposable, CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled) throw new ArgumentException("Require CancellationToken CanBeCanceled");

        if (cancellationToken.IsCancellationRequested)
        {
            disposable.Dispose();
            return default;
        }

        return cancellationToken.UnsafeRegister(state =>
        {
            var d = ((IDisposable)state!);
            d.Dispose();
        }, disposable);
    }

    public static IDisposable Create(Action onDisposed)
    {
        return new AnonymousDisposable(onDisposed);
    }

    public static IDisposable Create<T>(T state, Action<T> onDisposed)
    {
        return new AnonymousDisposable<T>(state, onDisposed);
    }

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2)
    {
        return new CombinedDisposable2(disposable1, disposable2);
    }

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
    {
        return new CombinedDisposable3(disposable1, disposable2, disposable3);
    }

    public static IDisposable Combine(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4
        )
    {
        return new CombinedDisposable4(
            disposable1,
            disposable2,
            disposable3,
            disposable4
            );
    }

    public static IDisposable Combine(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5
        )
    {
        return new CombinedDisposable5(
            disposable1,
            disposable2,
            disposable3,
            disposable4,
            disposable5
            );
    }

    public static IDisposable Combine(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6
        )
    {
        return new CombinedDisposable6(
            disposable1,
            disposable2,
            disposable3,
            disposable4,
            disposable5,
            disposable6
            );
    }

    public static IDisposable Combine(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6,
        IDisposable disposable7
        )
    {
        return new CombinedDisposable7(
            disposable1,
            disposable2,
            disposable3,
            disposable4,
            disposable5,
            disposable6,
            disposable7
            );
    }

    public static IDisposable Combine(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6,
        IDisposable disposable7,
        IDisposable disposable8
        )
    {
        return new CombinedDisposable8(
            disposable1,
            disposable2,
            disposable3,
            disposable4,
            disposable5,
            disposable6,
            disposable7,
            disposable8
            );
    }

    public static IDisposable Combine(params IDisposable[] disposables)
    {
        return new CombinedDisposable(disposables);
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2)
    {
        disposable1.Dispose();
        disposable2.Dispose();
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();

    }

    public static void Dispose(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4
        )
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
    }

    public static void Dispose(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5
        )
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
    }

    public static void Dispose(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6
        )
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
    }

    public static void Dispose(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6,
        IDisposable disposable7
        )
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
    }

    public static void Dispose(
        IDisposable disposable1,
        IDisposable disposable2,
        IDisposable disposable3,
        IDisposable disposable4,
        IDisposable disposable5,
        IDisposable disposable6,
        IDisposable disposable7,
        IDisposable disposable8
        )
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
        disposable8.Dispose();
    }

    public static void Dispose(params IDisposable[] disposables)
    {
        foreach (var item in disposables)
        {
            item.Dispose();
        }
    }
}

internal sealed class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

internal sealed class CombinedDisposable2(IDisposable disposable1, IDisposable disposable2) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
    }
}

internal sealed class CombinedDisposable3(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
    }
}

internal sealed class CombinedDisposable4(
    IDisposable disposable1,
    IDisposable disposable2,
    IDisposable disposable3,
    IDisposable disposable4) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
    }
}


internal sealed class CombinedDisposable5(
    IDisposable disposable1,
    IDisposable disposable2,
    IDisposable disposable3,
    IDisposable disposable4,
    IDisposable disposable5) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
    }
}


internal sealed class CombinedDisposable6(
    IDisposable disposable1,
    IDisposable disposable2,
    IDisposable disposable3,
    IDisposable disposable4,
    IDisposable disposable5,
    IDisposable disposable6) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
    }
}

internal sealed class CombinedDisposable7(
    IDisposable disposable1,
    IDisposable disposable2,
    IDisposable disposable3,
    IDisposable disposable4,
    IDisposable disposable5,
    IDisposable disposable6,
    IDisposable disposable7) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
    }
}

internal sealed class CombinedDisposable8(
    IDisposable disposable1,
    IDisposable disposable2,
    IDisposable disposable3,
    IDisposable disposable4,
    IDisposable disposable5,
    IDisposable disposable6,
    IDisposable disposable7,
    IDisposable disposable8) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
        disposable8.Dispose();
    }
}

internal sealed class CombinedDisposable(IDisposable[] disposables) : IDisposable
{
    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}

public ref struct DisposableBuilder()
{
    IDisposable? disposable1;
    IDisposable? disposable2;
    IDisposable? disposable3;
    IDisposable? disposable4;
    IDisposable? disposable5;
    IDisposable? disposable6;
    IDisposable? disposable7;
    IDisposable? disposable8;
    IDisposable[]? disposables;

    int count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(IDisposable disposable)
    {
        ThrowHelper.ThrowArgumentNullIfNull(disposable);
        ThrowHelper.ThrowObjectDisposedIf(count == -1, typeof(DisposableBuilder));

        switch (count)
        {
            case 0:
                disposable1 = disposable;
                break;
            case 1:
                disposable2 = disposable;
                break;
            case 2:
                disposable3 = disposable;
                break;
            case 3:
                disposable4 = disposable;
                break;
            case 4:
                disposable5 = disposable;
                break;
            case 5:
                disposable6 = disposable;
                break;
            case 6:
                disposable7 = disposable;
                break;
            case 7:
                disposable8 = disposable;
                break;
            default:
                AddToArray(disposable);
                break;
        }

        count++;
    }

    void AddToArray(IDisposable disposable)
    {
        if (count == 8)
        {
            var newDisposables = ArrayPool<IDisposable>.Shared.Rent(16);
            newDisposables[8] = disposable; // JIT optimize
            newDisposables[0] = disposable1!;
            newDisposables[1] = disposable2!;
            newDisposables[2] = disposable3!;
            newDisposables[3] = disposable4!;
            newDisposables[4] = disposable5!;
            newDisposables[5] = disposable6!;
            newDisposables[6] = disposable7!;
            newDisposables[7] = disposable8!;
            disposable1 = disposable2 = disposable3 = disposable4 = disposable5 = disposable6 = disposable7 = disposable8 = null;
            disposables = newDisposables; // assign
        }
        else
        {
            if (disposables!.Length == count)
            {
                var newDisposables = ArrayPool<IDisposable>.Shared.Rent(count * 2);
                Array.Copy(disposables, newDisposables, disposables.Length);
                ArrayPool<IDisposable>.Shared.Return(disposables, clearArray: true);
                disposables = newDisposables;
            }
            disposables[count] = disposable;
        }
    }

    public IDisposable Build()
    {
        ThrowHelper.ThrowObjectDisposedIf(count == -1, typeof(DisposableBuilder));

        IDisposable result;
        switch (count)
        {
            case 0:
                result = Disposable.Empty;
                break;
            case 1:
                result = disposable1!;
                break;
            case 2:
                result = new CombinedDisposable2(
                    disposable1!,
                    disposable2!
                );
                break;
            case 3:
                result = new CombinedDisposable3(
                    disposable1!,
                    disposable2!,
                    disposable3!
                );
                break;
            case 4:
                result = new CombinedDisposable4(
                    disposable1!,
                    disposable2!,
                    disposable3!,
                    disposable4!
                );
                break;
            case 5:
                result = new CombinedDisposable5(
                    disposable1!,
                    disposable2!,
                    disposable3!,
                    disposable4!,
                    disposable5!
                );
                break;
            case 6:
                result = new CombinedDisposable6(
                    disposable1!,
                    disposable2!,
                    disposable3!,
                    disposable4!,
                    disposable5!,
                    disposable6!
                );
                break;
            case 7:
                result = new CombinedDisposable7(
                    disposable1!,
                    disposable2!,
                    disposable3!,
                    disposable4!,
                    disposable5!,
                    disposable6!,
                    disposable7!
                );
                break;
            case 8:
                result = new CombinedDisposable8(
                    disposable1!,
                    disposable2!,
                    disposable3!,
                    disposable4!,
                    disposable5!,
                    disposable6!,
                    disposable7!,
                    disposable8!
                );
                break;
            default:
                result = new CombinedDisposable(disposables!.AsSpan(0, count).ToArray());
                break;
        }

        Dispose();
        return result;
    }

    public CancellationTokenRegistration RegisterTo(CancellationToken cancellationToken)
    {
        return Build().RegisterTo(cancellationToken);
    }

    public void Dispose()
    {
        if (count != -1)
        {
            disposable1 = disposable2 = disposable3 = disposable4 = disposable5 = disposable6 = disposable7 = disposable8 = null;
            if (disposables != null)
            {
                ArrayPool<IDisposable>.Shared.Return(disposables, clearArray: true);
            }
            count = -1;
        }
    }
}

internal sealed class AnonymousDisposable : IDisposable
{
    volatile Action? onDisposed;

    // public bool IsDisposed => onDisposed == null;

    public AnonymousDisposable(Action onDisposed)
    {
        this.onDisposed = onDisposed;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref onDisposed, null)?.Invoke();
    }
}

internal sealed class AnonymousDisposable<T> : IDisposable
{
    T state;
    volatile Action<T>? onDisposed;

    // public bool IsDisposed => onDisposed == null;

    public AnonymousDisposable(T state, Action<T> onDisposed)
    {
        this.state = state;
        this.onDisposed = onDisposed;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref onDisposed, null)?.Invoke(state);
        state = default!;
    }
}
