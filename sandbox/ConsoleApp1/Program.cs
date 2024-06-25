using R3;

// check https://github.com/Cysharp/R3/issues/226

//Observable.ReturnUnit()
//  .SelectMany(selector: _ => Observable
//    .Defer(observableFactory: () => Observable.ReturnUnit().SubscribeOnThreadPool())
//    // .SubscribeOnThreadPool()
//  ) // Observable<Unit>
//  .Subscribe();


Observable.Defer(observableFactory: () => Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(1))).Subscribe();

Console.ReadLine();
