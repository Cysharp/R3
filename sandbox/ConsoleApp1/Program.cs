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



var ct = new CancellationTokenSource(1000);
ObservableSystem.DefaultFrameProvider = new ThreadSleepFrameProvider(60);


// Enumerable.Empty<int>().ElementAtOrDefault(

var range = System.Reactive.Linq.Observable.Range(1, 10);

// range.TakeLast(


var publisher = new R3.Subject<int>();
//publisher.PublishOnNext(1);

// publisher.Subscribe(new object(), (x,y) => y

//var xs = await publisher.Take(TimeSpan.FromSeconds(5));

foreach (var item in Enumerable.Range(1, 10).TakeWhile(x => x <= 3))
{
    Console.WriteLine(item);
}

var repeat = System.Reactive.Linq.Observable.Repeat("foo", 10);
// repeat.TakeWhile(

// System.Reactive.Linq.Observable.FromEvent(

var rp = new ReactiveProperty<int>(999);
rp.Value += 10;

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

