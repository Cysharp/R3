using R3;
using System.Threading.Channels;
using System.Xml.Serialization;



var current = ObservableSystem.GetUnhandledExceptionHandler();



var status = Observable.Interval(TimeSpan.FromMilliseconds(100)).Index();
var doSomething = Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(5);

status.TakeUntil(doSomething.TakeLast(1)).Subscribe(_ => { }, ex =>
{
    Console.WriteLine("E" + ex);
}, r =>
{
    Console.WriteLine("R" + r);
});


Console.ReadLine()
;
