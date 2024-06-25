namespace R3.Tests.FactoryTests;

public class DeferTest
{
    [Fact]
    public void Test()
    {
        var called = false;
        var def = Observable.Defer(() =>
        {
            called = true;
            return Observable.Range(1, 10);
        });

        called.Should().BeFalse();

        var list = def.ToLiveList();

        called.Should().BeTrue();

        list.AssertEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
    }

    [Fact]
    public void SubscribeInDefer()
    {
        Observable.ReturnUnit()
            .SelectMany(selector: _ => Observable
            .Defer(observableFactory: () => Observable.ReturnUnit().SubscribeOnThreadPool())
            .SubscribeOnThreadPool()
            )
            .Subscribe();

        Observable.Defer(observableFactory: () => Observable.ReturnUnit().SubscribeOnThreadPool()).Subscribe();
        Observable.Defer(observableFactory: () => Observable.ReturnUnit().Delay(TimeSpan.FromSeconds(1))).Subscribe();

        Thread.Sleep(TimeSpan.FromSeconds(1.5));
    }
}


