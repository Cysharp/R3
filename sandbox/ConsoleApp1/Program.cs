using R2;
using R2.Internal;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Channels;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;

var s = new System.Reactive.Subjects.Subject<int>();


EventFactory.Timer(TimeSpan.FromSeconds(3), TimeProvider.System)
    .WriteLine();

Console.ReadLine();

var a = new ReactiveProperty<int>(100);
var b = new ReactiveProperty<int>(999);


a.CombineLatest(b, (x, y) => (x, y)).WriteLine();


a.Value = 3;
a.Value = 4;
b.Value = 99999;
b.Value = 1111;



//Observable.event




public static class Extensions
{
    public static IDisposable WriteLine<T>(this IEvent<T> source)
    {
        return source.Subscribe(x => Console.WriteLine(x));
    }

    public static IDisposable WriteLine<T, U>(this ICompletableEvent<T, U> source)
    {
        return source.Subscribe(x => Console.WriteLine(x), _ => Console.WriteLine("COMPLETED"));
    }
}
