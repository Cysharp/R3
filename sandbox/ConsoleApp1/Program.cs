using R3;
//using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Channels;
using System.Xml.Serialization;


var b = new Subject<bool>();



var rp = new ReactiveCommand<int, string>(async (x, ct) =>
{
    await Task.Delay(TimeSpan.FromSeconds(1));
    return x + "foo";
});

rp.Subscribe(x => Console.WriteLine("a:" + x));
rp.Subscribe(x => Console.WriteLine("b:" + x));

rp.Execute(0);
rp.Execute(1);

Console.ReadLine();

rp.Dispose();
