namespace R3.Tests.OperatorTests;

public class MulticastTest
{
    [Fact]
    public void Publish()
    {
        var subject = new Subject<int>();

        var connectable = subject.Publish();

        var list1 = connectable.ToLiveList();
        var list2 = connectable.ToLiveList();

        subject.OnNext(100);

        var connection = connectable.Connect();

        subject.OnNext(110);
        subject.OnNext(120);
        subject.OnNext(130);

        list1.AssertEqual([110, 120, 130]);
        list2.AssertEqual([110, 120, 130]);

        connection.Dispose();

        subject.OnCompleted();

        list1.AssertIsNotCompleted();
        list2.AssertIsNotCompleted();

        var reconnection = connectable.Connect();

        list1.AssertIsCompleted();
        list2.AssertIsCompleted();

        reconnection.Dispose();
    }
}
