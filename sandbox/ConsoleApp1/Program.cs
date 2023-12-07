using R2;
using R2.Internal;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Channels;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;

var s = new CompletablePublisher<int, Unit>();

var d1 = s.Subscribe((x) => Console.WriteLine(x));
var d2 = s.Subscribe((x) => Console.WriteLine(x));
var d3 = s.Subscribe((x) => Console.WriteLine(x));
var d4 = s.Subscribe((x) => Console.WriteLine(x));
var d5 = s.Subscribe((x) => Console.WriteLine(x));
// var d6 = s.Subscribe((x) => Console.WriteLine(x));

s.OnNext(1);

d3.Dispose();
d4.Dispose();

d2.Dispose();
d1.Dispose();
d5.Dispose();

//var xs = new[] { 1, 10, 3, 4, 5, 0, 7, 8, 9, 10 };

//xs.AsSpan().LastIndexOfAnyExcept(0);


////var gate = new object();
////var list = new CompactListCore<object>(gate);

////object a = new object();
////object b = new object();
////object c = new object();
////list.Add(a);
////list.Add(b);
////list.Add(c);

////list.Remove(b);


////d.Dispose();
//public class Foo
//{

//}
