namespace R3.Tests.OperatorTests;

public class RefCountTest
{
    [Fact]
    public void RefCount()
    {
        var subject = new Subject<int>();
        var refCounted = subject.Publish().RefCount(); // same as Share
        var list1 = refCounted.ToLiveList();
        var list2 = refCounted.ToLiveList();

        subject.OnNext(10);
        list1.AssertEqual([10]);
        list2.AssertEqual([10]);

        subject.OnNext(20);
        list1.AssertEqual([10, 20]);
        list2.AssertEqual([10, 20]);

        list1.Dispose();
        list2.Dispose();

        var list3 = refCounted.ToLiveList();

        subject.OnNext(30);
        subject.OnCompleted();

        list3.AssertEqual([30]);
        list3.AssertIsCompleted();
    }
}
