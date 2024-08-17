namespace R3.Tests;

public class SubscriptionTests
{
    public class TestObservable<T> : Observable<T>, IDisposable
    {
        private readonly Subject<T> _subject = new();

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }

    [Fact]
    public void Subscribe_ShouldNotThrowException()
    {
        var subject = new TestObservable<object>();
        IDisposable disposable = null;
        Action action = () =>
        {
            disposable = subject.Subscribe(_ => { });
        };
        action.Should().NotThrow();
        disposable?.Dispose();
    }
}
