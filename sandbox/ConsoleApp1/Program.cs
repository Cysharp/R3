using R3;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;


Console.WriteLine("hello");
// System.Linq.AsyncEnumerable.Range(1,10)


//Dump.Factory();

//System.Reactive.Linq.Observable.Range(1,10).SelectMany(

//var onClick = new Subject<Unit>();
//var httpClient = new HttpClient();


//onClick.SelectAwait(async x =>
//{


//});



Observable.Create<int>(observer =>
{
    observer.OnNext(1);

    return Disposable.Empty;
});

Observable.Create<int>(async (observer, ct) =>
{
    observer.OnNext(1);
    await Task.Delay(1000, ct);
});

Observable.CreateFrom(Gen);

static async IAsyncEnumerable<int> Gen([EnumeratorCancellation] CancellationToken ct)
{
    yield return 1;
    await Task.Delay(1000, ct);
    yield return 2;
    await Task.Delay(1000, ct);
    yield return 3;
}
