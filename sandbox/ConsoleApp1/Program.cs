using Microsoft.Extensions.Logging;
using R3;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Channels;
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




var sw = Stopwatch.StartNew();
var subject1 = new System.Reactive.Subjects.Subject<int>();
var subject2 = new System.Reactive.Subjects.Subject<int>();
subject1.WithLatestFrom(subject2.Finally(() => Console.WriteLine("finally subject2")), (x, y) => (x, y)).Subscribe(x => Console.WriteLine(x), () => Console.WriteLine("end"));

subject1.OnNext(1);
subject1.OnNext(10);
subject1.OnNext(100);


// subject2.OnNext(2);

subject1.OnNext(1000);

// subject2.OnError(new Exception());

subject1.OnNext(100000);
subject1.OnNext(1000000);
subject1.OnNext(10000000);
subject1.OnNext(100000000);

subject1.OnCompleted();




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

