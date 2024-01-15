
using System.Numerics;

namespace R3;

public static partial class ObservableExtensions
{
    public static Task<double> AverageAsync(this Observable<int> source, CancellationToken cancellationToken = default)
    {
        var method = new AverageInt32Async(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<T>(this Observable<T> source, Func<T, int> selector, CancellationToken cancellationToken = default)
    {
        var method = new AverageInt32Async<T>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync(this Observable<long> source, CancellationToken cancellationToken = default)
    {
        var method = new AverageInt64Async(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<T>(this Observable<T> source, Func<T, long> selector, CancellationToken cancellationToken = default)
    {
        var method = new AverageInt64Async<T>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync(this Observable<float> source, CancellationToken cancellationToken = default)
    {
        var method = new AverageFloatAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<T>(this Observable<T> source, Func<T, float> selector, CancellationToken cancellationToken = default)
    {
        var method = new AverageFloatAsync<T>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync(this Observable<double> source, CancellationToken cancellationToken = default)
    {
        var method = new AverageDoubleAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<T>(this Observable<T> source, Func<T, double> selector, CancellationToken cancellationToken = default)
    {
        var method = new AverageDoubleAsync<T>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync(this Observable<decimal> source, CancellationToken cancellationToken = default)
    {
        var method = new AverageDecimalAsync(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<T>(this Observable<T> source, Func<T, decimal> selector, CancellationToken cancellationToken = default)
    {
        var method = new AverageDecimalAsync<T>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

#if NET8_0_OR_GREATER
    public static Task<double> AverageAsync<T>(this Observable<T> source, CancellationToken cancellationToken = default)
        where T : INumberBase<T>
    {
        var method = new AverageNumberAsync<T>(cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }

    public static Task<double> AverageAsync<TSource, TResult>(this Observable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default)
        where TResult : INumberBase<TResult>
    {
        var method = new AverageNumberAsync<TSource, TResult>(selector, cancellationToken);
        source.Subscribe(method);
        return method.Task;
    }
#endif
}

internal sealed class AverageInt32Async(CancellationToken cancellationToken) : TaskObserverBase<int, double>(cancellationToken)
{
    int sum;
    int count;

    protected override void OnNextCore(int value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}

internal sealed class AverageInt32Async<TSource>(Func<TSource, int> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    int sum;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum += selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}
internal sealed class AverageInt64Async(CancellationToken cancellationToken) : TaskObserverBase<long, double>(cancellationToken)
{
    long sum;
    int count;

    protected override void OnNextCore(long value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}

internal sealed class AverageInt64Async<TSource>(Func<TSource, long> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    long sum;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum += selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}
internal sealed class AverageFloatAsync(CancellationToken cancellationToken) : TaskObserverBase<float, double>(cancellationToken)
{
    float sum;
    int count;

    protected override void OnNextCore(float value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}

internal sealed class AverageFloatAsync<TSource>(Func<TSource, float> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    float sum;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum += selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}
internal sealed class AverageDoubleAsync(CancellationToken cancellationToken) : TaskObserverBase<double, double>(cancellationToken)
{
    double sum;
    int count;

    protected override void OnNextCore(double value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(sum / count);
    }
}

internal sealed class AverageDoubleAsync<TSource>(Func<TSource, double> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    double sum;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum += selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(sum / count);
    }
}
internal sealed class AverageDecimalAsync(CancellationToken cancellationToken) : TaskObserverBase<decimal, double>(cancellationToken)
{
    decimal sum;
    int count;

    protected override void OnNextCore(decimal value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}

internal sealed class AverageDecimalAsync<TSource>(Func<TSource, decimal> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
{
    decimal sum;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum += selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        TrySetResult(checked((double)sum) / count);
    }
}

#if NET8_0_OR_GREATER
internal sealed class AverageNumberAsync<T>(CancellationToken cancellationToken) : TaskObserverBase<T, double>(cancellationToken)
    where T : INumberBase<T>
{
    T sum = T.Zero;
    int count;

    protected override void OnNextCore(T value)
    {
        sum += value;
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        double numerator;
        try
        {
            numerator = double.CreateChecked(sum);
        }
        catch (Exception ex)
        {
            TrySetException(ex);
            return;
        }
        TrySetResult(numerator / count);
    }
}

internal sealed class AverageNumberAsync<TSource, TResult>(Func<TSource, TResult> selector, CancellationToken cancellationToken) : TaskObserverBase<TSource, double>(cancellationToken)
    where TResult : INumberBase<TResult>
{
    TResult sum = TResult.Zero;
    int count;

    protected override void OnNextCore(TSource value)
    {
        sum = sum + selector(value);
        count = checked(count + 1);
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

        if (count <= 0)
        {
            TrySetException(new InvalidOperationException("Sequence contains no elements"));
            return;
        }

        double numerator;
        try
        {
            numerator = double.CreateChecked(sum);
        }
        catch (Exception ex)
        {
            TrySetException(ex);
            return;
        }

        TrySetResult(numerator / count);
    }
}
#endif
