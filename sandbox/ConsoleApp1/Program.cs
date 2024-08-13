using R3;
using System.Diagnostics;
using System.Threading.Channels;
using System.Xml.Serialization;



string[] array = { "a", "b", "c" };
array
    .ToObservable()
    .SubscribeAwait(static async (element, token) =>
    {
        Console.WriteLine(element);
        await Task.Yield();
    }, AwaitOperation.Sequential);


Console.ReadLine();
