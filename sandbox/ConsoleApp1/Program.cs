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
EventSystem.Logger = factory.CreateLogger<EventSystem>();
var logger = factory.CreateLogger<Program>();



var ct = new CancellationTokenSource(1000);
EventSystem.DefaultFrameProvider = new ThreadSleepFrameProvider(60);


// Enumerable.Empty<int>().ElementAtOrDefault(

var i = Enumerable.Range(4, 10).ElementAtOrDefault(^0);
Console.WriteLine(i);



IEnumerable<int> Range(int count)
{
    for (int i = 0; i < count; i++)
    {
        Console.WriteLine(i);
        yield return i;
    }
}


public static class Extensions
{
    public static IDisposable WriteLine<T>(this Event<T> source)
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

