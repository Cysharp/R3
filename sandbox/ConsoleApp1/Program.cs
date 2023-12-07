using R2;
//using System.Reactive.Disposables;
//using System.Reactive.Subjects;
//using System.Threading.Channels;

//var s = new Subject<int>();
// var s = new CompletablePublisher<int, Unit>();
var s = new Flow<int>(999);



var builder = Disposable.CreateBuilder();



s.Subscribe(x => Console.WriteLine(x)).AddTo(ref builder);
s.Subscribe(x => Console.WriteLine(x)).AddTo(ref builder);
s.Subscribe(x => Console.WriteLine(x)).AddTo(ref builder);
s.Subscribe(x => Console.WriteLine(x)).AddTo(ref builder);
s.Subscribe(x => Console.WriteLine(x)).AddTo(ref builder);







var d = builder.Build();


s.Value = 1;
s.Value = 1;
s.Value = 1;
s.Value = 1;
s.Value = 1;
s.Value = 1;


s.Value = 10;
// System.Reactive.Disposables.StableCompositeDisposable
