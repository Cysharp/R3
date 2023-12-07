using System.Buffers;
using System.Runtime.CompilerServices;

namespace R2;

public static class Disposable
{
    public static readonly IDisposable Empty = new EmptyDisposable();

    public static DisposableBuilder CreateBuilder()
    {
        return new DisposableBuilder(4);
    }

    public static DisposableBuilder CreateBuilder(int initialCapacity)
    {
        return new DisposableBuilder(initialCapacity);
    }

    public static void AddTo(this IDisposable disposable, ref DisposableBuilder builder)
    {
        builder.Add(disposable);
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
}

internal class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

internal class CombinedDisposable2(IDisposable disposable1, IDisposable disposable2) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
    }
}

internal class CombinedDisposable3(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) : IDisposable
{
    public void Dispose()
    {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
    }
}

internal class CombinedDisposable4(
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


internal class CombinedDisposable5(
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


internal class CombinedDisposable6(
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

internal class CombinedDisposable7(
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

internal class CombinedDisposable8(
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
    IDisposable[] disposables;
    int count;

    public DisposableBuilder(int initialCapacity)
        : this()
    {
        disposables = ArrayPool<IDisposable>.Shared.Rent(initialCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(IDisposable disposable)
    {
        ObjectDisposedException.ThrowIf(disposables == null, typeof(DisposableBuilder));

        if (count == disposables.Length)
        {
            Grow();
        }

        disposables[disposables.Length] = disposable;
        count++;
    }

    void Grow()
    {
        var newDisposables = ArrayPool<IDisposable>.Shared.Rent((disposables.Length == 0) ? 4 : disposables.Length * 2);
        Array.Copy(disposables, newDisposables, disposables.Length);
        ArrayPool<IDisposable>.Shared.Return(disposables);
        disposables = newDisposables;
    }

    public IDisposable Build()
    {
        ObjectDisposedException.ThrowIf(disposables == null, typeof(DisposableBuilder));

        IDisposable result;
        switch (count)
        {
            case 0:
                result = Disposable.Empty;
                break;
            case 1:
                result = disposables[0];
                break;
            case 2:
                result = new CombinedDisposable2(
                    disposables[0],
                    disposables[1]
                );
                break;
            case 3:
                result = new CombinedDisposable3(
                    disposables[0],
                    disposables[1],
                    disposables[2]
                );
                break;
            case 4:
                result = new CombinedDisposable4(
                    disposables[0],
                    disposables[1],
                    disposables[2],
                    disposables[3]
                );
                break;
            case 5:
                result = new CombinedDisposable5(
                    disposables[0],
                    disposables[1],
                    disposables[2],
                    disposables[3],
                    disposables[4]
                );
                break;
            case 6:
                result = new CombinedDisposable6(
                    disposables[0],
                    disposables[1],
                    disposables[2],
                    disposables[3],
                    disposables[4],
                    disposables[5]
                );
                break;
            case 7:
                result = new CombinedDisposable7(
                    disposables[0],
                    disposables[1],
                    disposables[2],
                    disposables[3],
                    disposables[4],
                    disposables[5],
                    disposables[6]
                );
                break;
            case 8:
                result = new CombinedDisposable8(
                    disposables[0],
                    disposables[1],
                    disposables[2],
                    disposables[3],
                    disposables[4],
                    disposables[5],
                    disposables[6],
                    disposables[7]
                );
                break;
            default:
                result = new CombinedDisposable(disposables);
                break;
        }

        disposables = null!;
        return result;
    }

    public void Dispose()
    {
        if (disposables != null)
        {
            ArrayPool<IDisposable>.Shared.Return(disposables);
            disposables = null!;
        }
    }
}