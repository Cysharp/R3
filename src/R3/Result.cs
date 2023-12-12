using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace R3;

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value, null);
    public static Result<Unit> Failure(Exception exception) => new(default, exception);
    public static Result<T> Failure<T>(Exception exception) => new(default, exception);
}

public readonly struct Result<T>(T? value, Exception? exception)
{
    public readonly T? Value = value;
    public readonly Exception? Exception = exception;

    [MemberNotNullWhen(false, nameof(Exception))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess => Exception == null;

    [MemberNotNullWhen(true, nameof(Exception))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => Exception != null;

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        if (IsSuccess)
        {
            value = Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryGetException([NotNullWhen(true)] out Exception? exception)
    {
        if (IsFailure)
        {
            exception = Exception;
            return true;
        }
        else
        {
            exception = default;
            return false;
        }
    }

    public void TryThrow()
    {
        if (IsFailure)
        {
            ExceptionDispatchInfo.Capture(Exception).Throw();
        }
    }

    public void Deconstruct(out T? value, out Exception? exception)
    {
        exception = Exception;
        value = Value;
    }

    public override string ToString()
    {
        if (IsSuccess)
        {
            return $"Success{{{Value}}}";
        }
        else
        {
            return $"Failure{{{Exception.Message}}}";
        }
    }
}
