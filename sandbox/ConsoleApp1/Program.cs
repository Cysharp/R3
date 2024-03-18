using R3;

var subject = new Subject<Observable<int>>();

var disposable = subject.Switch().Subscribe();

var observable1 = new Subject<int>();
var observable2 = new Subject<int>();
var observable3 = new Subject<int>();
var observable4 = new Subject<int>();

subject.OnNext(observable1.Do(onDispose: () => Console.WriteLine("Dispose 1")));
subject.OnNext(observable2.Do(onDispose: () => Console.WriteLine("Dispose 2")));
subject.OnNext(observable3.Do(onDispose: () => Console.WriteLine("Dispose 3")));
subject.OnNext(observable4.Do(onDispose: () => Console.WriteLine("Dispose 4")));

disposable.Dispose();
