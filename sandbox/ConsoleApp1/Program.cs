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
var subject = new System.Reactive.Subjects.Subject<int>();
subject.Sample(TimeSpan.FromSeconds(3)).Subscribe(x => Console.WriteLine(x));

subject.OnNext(1);

Console.ReadLine();

subject.OnNext(2);

subject.OnCompleted();





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

