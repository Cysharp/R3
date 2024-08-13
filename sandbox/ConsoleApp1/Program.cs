using R3;
using System.Threading.Channels;
using System.Xml.Serialization;



var status = Observable.Interval(TimeSpan.FromMilliseconds(100)).Index();
var doSomething = Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(5);
status.TakeUntil(doSomething.TakeLast(1)).Subscribe(Console.WriteLine, r => Console.WriteLine("end"));

await Task.Delay(TimeSpan.FromDays(1));
