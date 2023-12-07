using System.Diagnostics.CodeAnalysis;

namespace R2;

public readonly struct Result<T>(T? value, Exception? exception)
{
    public readonly T? Value = value;
    public readonly Exception? Exception = exception;

    [MemberNotNullWhen(false, nameof(Exception))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Exception == null;

    [MemberNotNullWhen(true, nameof(Exception))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool HasException => Exception != null;

    public void Deconstruct(out T? value, out Exception? exception)
    {
        exception = Exception;
        value = Value;
    }
}
