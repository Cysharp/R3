
namespace R3.Tests.FactoryTests;

public class CreateTest
{
    [Fact]
    public void Create()
    {
        var source = Observable.Create<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(10);
            observer.OnNext(100);
            observer.OnCompleted();
            return Disposable.Empty;
        });

        source.ToLiveList().AssertEqual([1, 10, 100]);
    }

    [Fact]
    public void CreateS()
    {
        using var publisher = new Subject<int>();
        var source = Observable.Create<int, Subject<int>>(publisher, (observer, state) =>
        {
            return state.Subscribe(new Wrap<int>(observer));
        });

        using var list = source.ToLiveList();

        publisher.OnNext(1);
        list.AssertEqual([1]);
        publisher.OnNext(10);
        list.AssertEqual([1, 10]);
        publisher.OnNext(100);
        list.AssertEqual([1, 10, 100]);

        publisher.OnCompleted();
        list.AssertIsCompleted();
    }
}

file class Wrap<T>(Observer<T> observer) : Observer<T>
{
    protected override void OnCompletedCore(Result result)
    {
        observer.OnCompleted(result);
    }

    protected override void OnErrorResumeCore(Exception error)
    {
        observer.OnErrorResume(error);
    }

    protected override void OnNextCore(T value)
    {
        observer.OnNext(value);
    }
}
