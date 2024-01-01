using Microsoft.Extensions.Logging;
using R3;
using System.Reactive.Linq;
using ZLogger;

SubscriptionTracker.EnableTracking = true;
SubscriptionTracker.EnableStackTrace = true;

using var factory = LoggerFactory.Create(x =>
{
    x.SetMinimumLevel(LogLevel.Trace);
    x.AddZLoggerConsole();
});
ObservableSystem.Logger = factory.CreateLogger<ObservableSystem>();
var logger = factory.CreateLogger<Program>();


// dvar rp = new ReactiveProperty<int>(9999);



 var rep = new System.Reactive.Subjects.ReplaySubject<int>();
//var rep = new System.Reactive.Subjects.BehaviorSubject<int>(10);

rep.OnNext(10);
rep.OnNext(100);
rep.OnNext(1000);
rep.OnCompleted();





//rep.TimeInterval()


public static class Extensions
{
    public static IDisposable WriteLine<T>(this Observable<T> source)
    {
        return source.Subscribe(x => Console.WriteLine(x), x => Console.WriteLine(x));
    }
}



class TestDisposable : IDisposable
{
    public int CalledCount = 0;

    public void Dispose()
    {
        CalledCount += 1;
    }
}

