using R3;

var r1 = Observable.Return(1);
var r2 = Observable.Interval(TimeSpan.FromSeconds(1)).Index();

r1.Concat(r2).Subscribe(Console.WriteLine);

await Task.Delay(TimeSpan.FromDays(1)); // wait


