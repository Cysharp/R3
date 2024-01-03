namespace R3.Tests.OperatorTests;

public class OnErrorResumeAsFailureTest
{

    [Fact]
    public void OnErrorResumeAsFailure()
    {
        var subject = new Subject<int>();
        var list = subject.OnErrorResumeAsFailure().ToLiveList();


        subject.OnNext(10);
        subject.OnErrorResume(new Exception("foo"));

        list.AssertEqual([10]);
        list.AssertIsCompleted();
        list.Result.Exception!.Message.Should().Be("foo");

    }
}
