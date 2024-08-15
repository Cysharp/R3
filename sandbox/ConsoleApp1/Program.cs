using R3;
//using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Channels;
using System.Xml.Serialization;



var doScan = Observable.FromAsync(async (token) =>
{
    Console.WriteLine("scan start");
    await Task.Delay(TimeSpan.FromSeconds(3));
    Console.WriteLine("scan end");
    return 5;
});

var doCalc = Observable.FromAsync(async (token) =>
{
    Console.WriteLine("calc start");
    await Task.Delay(TimeSpan.FromSeconds(3), token);
    Console.WriteLine("calc end");
    return 10;
});

var countDown = Observable.Interval(TimeSpan.FromMilliseconds(300)).Index().Select(v => v > 9 ? 9 : v);

var work = doScan.Select(_ => doCalc).Switch().Replay(1).RefCount();
countDown.TakeUntil(work.LastAsync()).Concat(work.TakeLast(1)).Subscribe(v => Console.WriteLine($"progress: {v}"));


Console.ReadLine();
