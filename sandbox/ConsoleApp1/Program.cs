//using R3;
//using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Channels;
using System.Xml.Serialization;
using R3;


// Observable.Range(1,10).MinBy(
// Enumerable.Range(1,10).MinBy(

var x = new BindableReactiveProperty<string>().EnableValidation(x =>
{
    return string.IsNullOrEmpty(x) ? new InvalidDataException("Invalid X") : null;
});

x.HasErrors.Dump("After initialized");  // (1) False

x.Value = string.Empty;
x.HasErrors.Dump("Assign empty");  // (2) True

x.Value = "xx";
x.HasErrors.Dump("Assign not null");  // (3) False


public static class Ext
{
    public static void Dump(this object o, string msg)
    {
        Console.WriteLine(o + ":" + msg);
    }
}
