namespace R3.Tests;

public class ReplaySubjectTest
{
    [Fact]
    public void ReplayAll()
    {
        var subject = new ReplaySubject<int>();
        foreach (var i in Enumerable.Range(0, 100))
        {
            subject.OnNext(i);
        }

        var list = subject.ToLiveList();
        list.AssertEqual(Enumerable.Range(0, 100).ToArray());

        list.Clear();
        subject.OnNext(9);
        list.AssertEqual([9]);

        subject.OnCompleted();
        list.AssertIsCompleted();

        var list2 = subject.ToLiveList();

        list2.AssertEqual(Enumerable.Range(0, 100).Append(9).ToArray());
        list2.AssertIsCompleted();
    }

    [Fact]
    public void ReplayCount()
    {
        var subject = new ReplaySubject<int>(bufferSize: 50);
        foreach (var i in Enumerable.Range(0, 50))
        {
            subject.OnNext(i);
        }

        {
            using var list = subject.ToLiveList();
            list.AssertEqual(Enumerable.Range(0, 50).ToArray());
        }
        {
            subject.OnNext(100);
            subject.OnNext(101);
            subject.OnNext(102);

            using var list = subject.ToLiveList();
            list.AssertEqual(Enumerable.Range(0, 50).Skip(3).Concat([100, 101, 102]).ToArray());
        }

        subject.OnCompleted();

        {
            using var list = subject.ToLiveList();
            list.AssertEqual(Enumerable.Range(0, 50).Skip(3).Concat([100, 101, 102]).ToArray());
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void ReplayTime()
    {
        var fakeTime = new FakeTimeProvider();

        var subject = new ReplaySubject<int>(TimeSpan.FromSeconds(3), fakeTime);

        subject.OnNext(10);
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        subject.ToLiveList().AssertEqual([10]);

        subject.OnNext(20);
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        subject.ToLiveList().AssertEqual([10, 20]);

        subject.OnNext(30);
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        var list = subject.ToLiveList();
        subject.ToLiveList().AssertEqual([20, 30]);

        subject.OnNext(40);
        subject.OnNext(50);
        subject.OnNext(60);

        fakeTime.Advance(TimeSpan.FromSeconds(2));
        subject.OnNext(70);

        subject.ToLiveList().AssertEqual([40, 50, 60, 70]);
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        subject.ToLiveList().AssertEqual([70]);

        subject.OnCompleted();
        subject.ToLiveList().AssertEqual([70]);
        subject.ToLiveList().AssertIsCompleted();
    }
}
