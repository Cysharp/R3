using Microsoft.Extensions.Logging;
using R3;
using R3.Internal;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using ZLogger;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;



SubscriptionTracker.EnableTracking = true;
SubscriptionTracker.EnableStackTrace = false;

using var factory = LoggerFactory.Create(x =>
{
    x.SetMinimumLevel(LogLevel.Trace);
    x.AddZLoggerConsole();
});
EventSystem.Logger = factory.CreateLogger<EventSystem>();
var logger = factory.CreateLogger<Program>();

var publisher = new Publisher<int>();

var d = publisher
    .Where(x => true)
    .Select(x => x)
    .Subscribe(x =>
    {
        logger.ZLogInformation($"OnNext: {x}");
    });

SubscriptionTracker.ForEachActiveTask(x =>
{
    // logger.ZLogInformation($"{x.TrackingId,3}: {Environment.NewLine}{x.StackTrace.Replace("R2.", "").Replace("C:\\MyGit\\R2\\sandbox\\ConsoleApp1\\", "").Replace("C:\\MyGit\\R2\\src\\R2\\", "")}");


    // logger.ZLogInformation($"{x.TrackingId,3}: {x.FormattedType}");
});

publisher.PublishOnNext(1);
publisher.PublishOnNext(2);
publisher.PublishOnNext(3);

d.Dispose();




var s = new System.Reactive.Subjects.Subject<string>();


// s.Where(

// new Result<int>(






foreach (var item in typeof(System.Reactive.Linq.Observable).GetMethods().Select(x => x.Name).Distinct().OrderBy(x => x))
{
    if (item == "ToString" || item == "Equals" || item == "GetHashCode" || item == "GetType")
    {
        continue;
    }
    Console.WriteLine("- [ ] " + item);
}























//p.PublishOnNext(4); 
//p.PublishOnNext(5);

//Console.WriteLine("-------------------------");


//SubscriptionTracker.ForEachActiveTask(x =>
//{
//    Console.WriteLine($"{x.TrackingId,3}: {x.FirstLine}");
//});



//EventFactory.Return(10, TimeProvider.System)
//    .WriteLine();

//Console.ReadLine();

//var a = new ReactiveProperty<int>(100);
//var b = new ReactiveProperty<int>(999);


//a.CombineLatest(b, (x, y) => (x, y)).WriteLine();


//a.Value = 3;
//a.Value = 4;
//b.Value = 99999;
//b.Value = 1111;



//Observable.event




public static class Extensions
{
    public static IDisposable WriteLine<T>(this Event<T> source)
    {
        return source.Subscribe(x => Console.WriteLine(x));
    }

    public static IDisposable WriteLine<T, U>(this CompletableEvent<T, U> source)
    {
        return source.Subscribe(x => Console.WriteLine(x), _ => Console.WriteLine("COMPLETED"));
    }
}

