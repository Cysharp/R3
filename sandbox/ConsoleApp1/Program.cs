using R3;
using System.Threading.Channels;
using System.Xml.Serialization;


var a = new Subject<int>();
var b = new Subject<int>();

a.Zip(b, (x, y) => (x, y)).Subscribe(x => Console.WriteLine(x), _ => Console.WriteLine("complete"));


a.OnNext(1);
b.OnNext(2);
a.OnNext(3);

a.OnCompleted();
b.OnNext(4);


b.OnNext(5);
b.OnNext(6);
b.OnNext(7);
