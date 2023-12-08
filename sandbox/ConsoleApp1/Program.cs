using R2;
using R2.Internal;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Channels;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;

var s = new Publisher<int>();

var d1 = s.DelayFrame(10, ThreadFrameProvider.Instance).Subscribe((x) => Console.WriteLine(x));

s.OnNext(99);
Console.ReadLine();

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
