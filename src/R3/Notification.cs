namespace R3;

public enum NotificationKind
{
    OnNext,
    OnErrorResume,
    OnCompleted
}

public readonly struct Notification<T>
{
    readonly NotificationKind kind;
    readonly T? value;
    readonly Exception? error;
    readonly Result? result;

    public NotificationKind Kind => kind;
    public T? Value => value;
    public Exception? Error => error;
    public Result? Result => result;

    public Notification(T value)
    {
        this.kind = NotificationKind.OnNext;
        this.value = value;
        this.error = null;
        this.result = default;
    }

    public Notification(Exception error)
    {
        this.kind = NotificationKind.OnErrorResume;
        this.value = default;
        this.error = error;
        this.result = default;
    }

    public Notification(Result result)
    {
        this.kind = NotificationKind.OnCompleted;
        this.value = default;
        this.error = default;
        this.result = result;
    }

    public override string? ToString()
    {
        switch (kind)
        {
            case NotificationKind.OnNext:
                return value!.ToString();
            case NotificationKind.OnErrorResume:
                return error!.ToString();
            case NotificationKind.OnCompleted:
                return result!.Value.ToString();
            default:
                return "";
        }
    }
}
