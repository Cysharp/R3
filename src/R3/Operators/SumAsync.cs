
namespace R3;

using System.Numerics;

public static partial class ObservableExtensions
{
    public static Task<int> SumAsync(this Observable<int> source, CancellationToken cancellationToken = default)
    {
        var method = new SumInt32Async(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<int> SumAsync<TSource>(this Observable<TSource> source, Func<TSource, int> selector, CancellationToken cancellationToken = default)
    {
        var method = new SumInt32Async<TSource>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<long> SumAsync(this Observable<long> source, CancellationToken cancellationToken = default)
    {
        var method = new SumInt64Async(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<long> SumAsync<TSource>(this Observable<TSource> source, Func<TSource, long> selector, CancellationToken cancellationToken = default)
    {
        var method = new SumInt64Async<TSource>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<float> SumAsync(this Observable<float> source, CancellationToken cancellationToken = default)
    {
        var method = new SumFloatAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<float> SumAsync<TSource>(this Observable<TSource> source, Func<TSource, float> selector, CancellationToken cancellationToken = default)
    {
        var method = new SumFloatAsync<TSource>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> SumAsync(this Observable<double> source, CancellationToken cancellationToken = default)
    {
        var method = new SumDoubleAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> SumAsync<TSource>(this Observable<TSource> source, Func<TSource, double> selector, CancellationToken cancellationToken = default)
    {
        var method = new SumDoubleAsync<TSource>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<decimal> SumAsync(this Observable<decimal> source, CancellationToken cancellationToken = default)
    {
        var method = new SumDecimalAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<decimal> SumAsync<TSource>(this Observable<TSource> source, Func<TSource, decimal> selector, CancellationToken cancellationToken = default)
    {
        var method = new SumDecimalAsync<TSource>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }


#if NET8_0_OR_GREATER
    public static Task<T> SumAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
        where T : INumberBase<T>
    {
        var method = new SumNumberAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<TResult> SumAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        where TResult : INumberBase<TResult>
    {
        var method = new SumNumberAsync<TSource, TResult>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
#endif
}

internal sealed class SumInt32Async(CancellationToken cancellationToken) : TaskObserverBase<int, int>(cancellationToken)
{
    int sum;

    protected override void OnNextCore(int value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(sum);
    }
}

internal sealed class SumInt32Async<TSource>(Func<TSource, int> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, int>(cancellationToken)
{
    int sum;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}
internal sealed class SumInt64Async(CancellationToken cancellationToken) : TaskObserverBase<long, long>(cancellationToken)
{
    long sum;

    protected override void OnNextCore(long value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(sum);
    }
}

internal sealed class SumInt64Async<TSource>(Func<TSource, long> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, long>(cancellationToken)
{
    long sum;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}
internal sealed class SumFloatAsync(CancellationToken cancellationToken) : TaskObserverBase<float, float>(cancellationToken)
{
    float sum;

    protected override void OnNextCore(float value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(sum);
    }
}

internal sealed class SumFloatAsync<TSource>(Func<TSource, float> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, float>(cancellationToken)
{
    float sum;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}
internal sealed class SumDoubleAsync(CancellationToken cancellationToken) : TaskObserverBase<double, double>(cancellationToken)
{
    double sum;

    protected override void OnNextCore(double value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(sum);
    }
}

internal sealed class SumDoubleAsync<TSource>(Func<TSource, double> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    double sum;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}
internal sealed class SumDecimalAsync(CancellationToken cancellationToken) : TaskObserverBase<decimal, decimal>(cancellationToken)
{
    decimal sum;

    protected override void OnNextCore(decimal value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }
        TrySetResult(sum);
    }
}

internal sealed class SumDecimalAsync<TSource>(Func<TSource, decimal> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, decimal>(cancellationToken)
{
    decimal sum;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}

#if NET8_0_OR_GREATER
internal sealed class SumNumberAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, T>(cancellationToken)
    where T : INumberBase<T>
{
    T sum = T.Zero;

    protected override void OnNextCore(T value)
    {
        sum = checked(sum + value);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}

internal sealed class SumNumberAsync<TSource, TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, TResult>(cancellationToken)
    where TResult : INumberBase<TResult>
{
    TResult sum = TResult.Zero;

    protected override void OnNextCore(TSource value)
    {
        var add = selector(value);
        sum = checked(sum + add);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        TrySetException(error);
    }

    protected override void OnCompletedCore(Result result)
    {
        if (result.IsFailure)
        {
            TrySetException(result.Exception);
            return;
        }

        TrySetResult(sum);
    }
}
#endif
