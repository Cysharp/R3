using ConsoleApp1;
using R3;
using System.ComponentModel.DataAnnotations;



//Dump.Factory();

var hoge = new Hoge();

hoge.MyProperty1.ErrorsChanged += MyProperty1_ErrorsChanged;
hoge.MyProperty2.ErrorsChanged += MyProperty2_ErrorsChanged;

void MyProperty1_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
{
    foreach (var item in hoge.MyProperty1.GetErrors(null!))
    {
        Console.WriteLine(item);
    }
}
void MyProperty2_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
{
    foreach (var item in hoge.MyProperty2.GetErrors(null!))
    {
        Console.WriteLine(item);
    }
}
hoge.MyProperty1.Value = 30;
hoge.MyProperty2.Value = 40;


//SubscriptionTracker.EnableTracking = true;
//SubscriptionTracker.EnableStackTrace = true;

//using var factory = LoggerFactory.Create(x =>
//{
//    x.SetMinimumLevel(LogLevel.Trace);
//    x.AddZLoggerConsole();
//});
//// ObservableSystem.Logger = factory.CreateLogger<ObservableSystem>();
//var logger = factory.CreateLogger<Program>();




//var sw = Stopwatch.StartNew();
//var subject1 = new System.Reactive.Subjects.Subject<int>();
//var subject2 = new System.Reactive.Subjects.Subject<int>();
////subject1.WithLatestFrom(subject2.Finally(() => Console.WriteLine("finally subject2")), (x, y) => (x, y)).Subscribe(x => Console.WriteLine(x), () => Console.WriteLine("end"));

//subject1.Scan((x, y) => x + y).Subscribe(x => Console.WriteLine(x), () => Console.WriteLine("end"));


//subject1.OnNext(1);
//subject1.OnNext(10);
////subject1.OnNext(10);
////subject1.OnNext(100);

//// subject1.SequenceEqual(


//// System.Reactive.Linq.Observable.Switch(


//public static class Extensions
//{
//    public static IDisposable WriteLine<T>(this Observable<T> source)
//    {
//        return source.Subscribe(x => Console.WriteLine(x), x => Console.WriteLine(x));
//    }
//}



//class TestDisposable : IDisposable
//{
//    public int CalledCount = 0;

//    public void Dispose()
//    {
//        CalledCount += 1;
//    }
//}


public class Hoge
{
    [Range(1, 10)]
    public BindableReactiveProperty<int> MyProperty1 { get; set; } = new BindableReactiveProperty<int>().EnableValidation<Hoge>();

    [Range(1, 10)]
    public BindableReactiveProperty<int> MyProperty2 { get; set; }

    public Hoge()
    {
        MyProperty2 = new BindableReactiveProperty<int>().EnableValidation(() => MyProperty2);
    }
}
