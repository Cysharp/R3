using System.Runtime.InteropServices;

namespace R3;

public enum NotificationKind : byte
{
    OnNext,
    OnErrorResume,
    OnCompleted
}

[StructLayout(LayoutKind.Auto)]
public readonly struct Notification<T>
{
    readonly NotificationKind kind;
    readonly T? value;
    readonly Exception? errorOrResultFailure;

    public NotificationKind Kind => kind;
    public T Value => value!;
    public Exception Error => errorOrResultFailure!;
    public Result Result => errorOrResultFailure == null ? R3.Result.Success : R3.Result.Failure(errorOrResultFailure);

    public Notification(T value)
    {
        this.kind = NotificationKind.OnNext;
        this.value = value;
        this.errorOrResultFailure = null;
    }

    public Notification(Exception error)
    {
        this.kind = NotificationKind.OnErrorResume;
        this.value = default;
        this.errorOrResultFailure = error;
    }

    public Notification(Result result)
    {
        this.kind = NotificationKind.OnCompleted;
        this.value = default;
        this.errorOrResultFailure = result.Exception;
    }

    public override string? ToString()
    {
        switch (kind)
        {
            case NotificationKind.OnNext:
                return value!.ToString();
            case NotificationKind.OnErrorResume:
                return Error!.ToString();
            case NotificationKind.OnCompleted:
                return Result.ToString();
            default:
                return "";
        }
    }
}
