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



var p = new Publisher<int>();

var d = p
    .Take(2)
    .Subscribe(x =>
{
    Console.WriteLine(x);
});


p.PublishOnNext(1);
p.PublishOnNext(2);
p.PublishOnNext(3);

// d.Dispose();

p.PublishOnNext(4);
p.PublishOnNext(5);




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

