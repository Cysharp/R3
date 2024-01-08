namespace R3.Tests.OperatorTests;

public class DelaySubscriptionTest
{
    [Fact]
    public void DelaySubscription()
    {
        var provider = new FakeTimeProvider();

        var publisher = new Subject<int>();
        var subscribed = false;
        var list = publisher
            .Do(onSubscribe: () => subscribed = true)
            .DelaySubscription(TimeSpan.FromSeconds(3), provider)
            .ToLiveList();

        subscribed.Should().BeFalse();
        publisher.OnNext(1);
        list.AssertEqual([]);

        provider.Advance(TimeSpan.FromSeconds(2));

        subscribed.Should().BeFalse();
        publisher.OnNext(2);
        list.AssertEqual([]);

        provider.Advance(TimeSpan.FromSeconds(1));

        subscribed.Should().BeTrue();
        publisher.OnNext(3);
        list.AssertEqual([3]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }

    [Fact]
    public void DelaySubscriptionFrame()
    {
        var provider = new FakeFrameProvider();

        var publisher = new Subject<int>();
        var subscribed = false;
        var list = publisher
            .Do(onSubscribe: () => subscribed = true)
            .DelaySubscriptionFrame(3, provider)
            .ToLiveList();

        subscribed.Should().BeFalse();
        publisher.OnNext(1);
        list.AssertEqual([]);

        provider.Advance(2);

        subscribed.Should().BeFalse();
        publisher.OnNext(2);
        list.AssertEqual([]);

        provider.Advance(1);

        subscribed.Should().BeTrue();
        publisher.OnNext(3);
        list.AssertEqual([3]);

        publisher.OnCompleted();

        list.AssertIsCompleted();
    }
}
