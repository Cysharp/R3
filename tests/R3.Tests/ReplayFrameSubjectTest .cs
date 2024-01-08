namespace R3.Tests;

public class ReplayFrameSubjectTest
{
    [Fact]
    public void ReplayTime()
    {
        var fakeTime = new FakeFrameProvider();

        var subject = new ReplayFrameSubject<int>((3), fakeTime);

        subject.OnNext(10);
        fakeTime.Advance((1));
        subject.ToLiveList().AssertEqual([10]);

        subject.OnNext(20);
        fakeTime.Advance((1));
        subject.ToLiveList().AssertEqual([10, 20]);

        subject.OnNext(30);
        fakeTime.Advance((1));

        var list = subject.ToLiveList();
        subject.ToLiveList().AssertEqual([20, 30]);

        subject.OnNext(40);
        subject.OnNext(50);
        subject.OnNext(60);

        fakeTime.Advance((2));
        subject.OnNext(70);

        subject.ToLiveList().AssertEqual([40, 50, 60, 70]);
        fakeTime.Advance((1));

        subject.ToLiveList().AssertEqual([70]);

        subject.OnCompleted();
        subject.ToLiveList().AssertEqual([70]);
        subject.ToLiveList().AssertIsCompleted();
    }
}
