using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace R3;

// default(Result) is Succeeded
public readonly struct Result
{
    public static Result Success => default;
    public static Result Failure(Exception exception) => new(exception);

    public Exception? Exception { get; }

    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => Exception == null;

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsFailure => Exception != null;

    public Result(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        this.Exception = exception;
    }

    public void TryThrow()
    {
        if (IsFailure)
        {
            ExceptionDispatchInfo.Capture(Exception).Throw();
        }
    }

    public override string ToString()
    {
        if (IsSuccess)
        {
            return $"Success";
        }
        else
        {
            return $"Failure{{{Exception.Message}}}";
        }
    }
}
