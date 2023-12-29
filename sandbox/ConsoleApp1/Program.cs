using Microsoft.Extensions.Logging;
using R3;
using R3.Internal;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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


var rp = new ReactiveProperty<int>(9999);



var ct = new CancellationTokenSource(1000);
ObservableSystem.DefaultFrameProvider = new ThreadSleepFrameProvider(60);


// Enumerable.Empty<int>().ElementAtOrDefault(

var publisher = new System.Reactive.Subjects.Subject<int>();

var connectable = publisher.Multicast(new System.Reactive.Subjects.Subject<int>());


connectable.Subscribe(x => Console.WriteLine(x));

var d= connectable.Connect();


publisher.OnNext(100);

d.Dispose();


//var d2 = connectable.Connect();

publisher.OnNext(200);

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

