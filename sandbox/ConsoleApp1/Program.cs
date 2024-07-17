using R3;
using System.Diagnostics;

// https://github.com/Cysharp/R3/issues/232


Observable.Concat(
    Observable.Return("Start"),
    SomeAsyncTask().ToObservable(),
    Observable.Return("End")
).Subscribe(r =>
{
    Console.WriteLine("Result:" + r);
});


Console.ReadLine();

async ValueTask<string> SomeAsyncTask()
{
    await Task.Delay(1000);
    return "result";
}
