//using R3;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Channels;
using System.Xml.Serialization;


// Observable.Range(1,10).MinBy(
// Enumerable.Range(1,10).MinBy(

var a = new System.Reactive.Subjects.BehaviorSubject<int>(0);

//var d = Disposable.CreateBuilder();

// a.OnNext(




a.Do(v => Debug.Log($"a {v}")).Do(v =>
{
    if (v < 20)
    {
        a.OnNext(v + 1);
    }
}).Subscribe(); //.AddTo(ref d);



Debug.Log($"a == 1: {a.Value == 1}"); // this should be 20, not 1!

a.OnNext(0); // this 

Debug.Log($"a == 20: {a.Value == 20}"); // this is correct




public static class Debug
{
    public static void Log(string msg)
    {
        Console.WriteLine(msg);
    }
}
