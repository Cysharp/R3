using Microsoft.Extensions.Logging;
using R3;
using R3.Internal;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using ZLogger;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;








var disposables = Enumerable.Range(1, 100).Select(x => new TestDisposable()).ToArray();
var composite = new System.Reactive.Disposables.CompositeDisposable(disposables);

foreach (var item in disposables)
{
    composite.Remove(item);
}













SubscriptionTracker.EnableTracking = true;
SubscriptionTracker.EnableStackTrace = true;

using var factory = LoggerFactory.Create(x =>
{
    x.SetMinimumLevel(LogLevel.Trace);
    x.AddZLoggerConsole();
});
EventSystem.Logger = factory.CreateLogger<EventSystem>();
var logger = factory.CreateLogger<Program>();



var publisher = new Publisher<int, R3.Unit>();

var d = publisher
    .Where(x => true)
    .Select(x =>
    {
        //if (x == 2) throw new Exception("e");
        return x;
    })
    .Take(5)
    .OnErrorAsComplete()
    .Subscribe(x =>
    {
        if (x == 2) throw new Exception("e");
        logger.ZLogInformation($"OnNext: {x}");
    }, e =>
    {
        logger.ZLogInformation($"failure resume");
    },
    x =>
    {
        //logger.ZLogInformation($"end:{x}");
        x.TryThrow();
    });

SubscriptionTracker.ForEachActiveTask(x =>
{
    // logger.ZLogInformation($"{x.TrackingId,3}: {Environment.NewLine}{x.StackTrace.Replace("R2.", "").Replace("C:\\MyGit\\R2\\sandbox\\ConsoleApp1\\", "").Replace("C:\\MyGit\\R2\\src\\R2\\", "")}");


    // logger.ZLogInformation($"{x.TrackingId,3}: {x.FormattedType}");
});

//publisher.PublishOnNext(1);
//publisher.PublishOnNext(2);
//publisher.PublishOnNext(3);

//publisher.PublishOnErrorResume(new Exception("ERROR"));

//publisher.PublishOnNext(4);
//publisher.PublishOnNext(5);
//publisher.PublishOnNext(6);
//publisher.PublishOnNext(7);


//d.Dispose();


var iiii = Enumerable.Range(1, 10).ElementAt(^12);
Console.WriteLine(iiii);


// System.Reactive.Linq.Observable.Empty<int>(

var s = new System.Reactive.Subjects.Subject<string>();

// Console.WriteLine($"Average: {Enumerable.Empty<int>().Average()}");

// s.ToListObservable();

// Observable.Throw(
// s.Where(

// new Result<int>(

// s.ObserveOn(
// Observable.FromEventPattern(



//foreach (var item in typeof(System.Reactive.Linq.Observable).GetMethods().Select(x => x.Name).Distinct().OrderBy(x => x))
//{
//    if (item == "ToString" || item == "Equals" || item == "GetHashCode" || item == "GetType")
//    {
//        continue;
//    }
//    Console.WriteLine("- [ ] " + item);
//}







var subject = new Subject<int>();

subject.OnCompleted();

Console.WriteLine("Subscribe");
subject.Subscribe(x => Console.WriteLine(x));
subject.OnNext(99);




// subject.ForEachAsync(






//p.PublishOnNext(4); 
//p.PublishOnNext(5);

//Console.WriteLine("-------------------------");


//SubscriptionTracker.ForEachActiveTask(x =>
//{
//    Console.WriteLine($"{x.TrackingId,3}: {x.FirstLine}");
//});



//Event.Return(10, TimeProvider.System)
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
    public static IDisposable WriteLine<T, U>(this Event<T, U> source)
    {
        return source.Subscribe(x => Console.WriteLine(x), _ => Console.WriteLine("COMPLETED"));
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
