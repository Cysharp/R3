using R3;
using System.Threading.Channels;




async ValueTask<string> Something(CancellationToken ct)
{
    Console.WriteLine("Starting Task");
    await Task.Delay(1000, ct);
    Console.WriteLine("Ending Task");
    Console.WriteLine(ct.IsCancellationRequested);
    return "result";
}

Subject<int> v = new Subject<int>();

var dd = v
.Take(1)
.SelectAwait(async (v, ct) => await Something(ct))
.Subscribe(v =>
{
    Console.WriteLine(v);
}, _ => Console.WriteLine("end"));

v.OnNext(0);


Console.ReadLine();
