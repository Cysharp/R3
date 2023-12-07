using R2;
using System.Reactive.Concurrency;
using System.Threading.Channels;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;

//var s = new Subject<int>();
// var s = new CompletablePublisher<int, Unit>();
var s = new Publisher<int>();




var safeProvider = new SafeTimerTimeProvider(ex => Console.WriteLine(ex), TimeProvider.System);


var d = s.Delay(TimeSpan.FromSeconds(3), safeProvider)
    .Subscribe(x =>
    {
        throw new Exception(x.ToString());
        // Console.WriteLine(x);
    });


s.OnNext(1);
s.OnNext(2);
s.OnNext(3);
s.OnNext(4);
s.OnNext(5);

_ = Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(_ =>
{
    s.OnNext(99);
});



Console.ReadLine();

//d.Dispose();


internal static class TimeProviderExtensions
{
    public static ITimer CreateStoppedTimer(this TimeProvider timeProvider, TimerCallback timerCallback, object? state)
    {
        return timeProvider.CreateTimer(timerCallback, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public static void RestartImmediately(this ITimer timer)
    {
        timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    }

    public static void InvokeOnce(this ITimer timer, TimeSpan dueTime)
    {
        timer.Change(dueTime, Timeout.InfiniteTimeSpan);
    }
}

