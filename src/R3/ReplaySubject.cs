namespace R3;

public sealed class ReplaySubject<T>
{
    public ReplaySubject()
         : this(int.MaxValue, TimeSpan.MaxValue, ObservableSystem.DefaultTimeProvider)
    {
    }

    public ReplaySubject(TimeProvider timeProvider)
        : this(int.MaxValue, TimeSpan.MaxValue, timeProvider)
    {
    }

    public ReplaySubject(int bufferSize)
        : this(bufferSize, TimeSpan.MaxValue, ObservableSystem.DefaultTimeProvider)
    {
    }

    public ReplaySubject(int bufferSize, TimeProvider timeProvider)
        : this(bufferSize, TimeSpan.MaxValue, timeProvider)
    {
    }

    public ReplaySubject(TimeSpan window)
        : this(int.MaxValue, window, ObservableSystem.DefaultTimeProvider)
    {
    }

    public ReplaySubject(TimeSpan window, TimeProvider timeProvider)
        : this(int.MaxValue, window, timeProvider)
    {
    }

    // full constructor
    public ReplaySubject(int bufferSize, TimeSpan window, TimeProvider timeProvider)
    {
    }
}

// TODO: ReplayFrameSubject?
// full constructor 2?
//public ReplaySubject(int bufferSize, int windowFrame, FrameProvider frameProvider)
//{
//}
