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

d2.Dispose();


//var gate = new object();
//var list = new CompactListCore<object>(gate);

//object a = new object();
//object b = new object();
//object c = new object();
//list.Add(a);
//list.Add(b);
//list.Add(c);

//list.Remove(b);


//d.Dispose();
public class Foo
{

}
