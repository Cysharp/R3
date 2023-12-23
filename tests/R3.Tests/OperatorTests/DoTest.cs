namespace R3.Tests.OperatorTests;

public class DoTest
{
    [Fact]
    public void Test()
    {
        var subject = new Subject<int>();

        List<int> onNext = new();
        List<Exception> onErrorResume = new();
        List<Result> onCompleted = new();
        bool disposeCalled = false;
        bool subscribeCalled = false;
        var source = subject.Do(onNext.Add, onErrorResume.Add, onCompleted.Add, () => disposeCalled = true, () => subscribeCalled = true);

        subscribeCalled.Should().BeFalse();

        var list = source.ToLiveList();

        subscribeCalled.Should().BeTrue();

        subject.OnNext(10);
        subject.OnNext(20);
        subject.OnNext(30);
        subject.OnErrorResume(new Exception("a"));
        subject.OnErrorResume(new Exception("b"));
        subject.OnErrorResume(new Exception("c"));

        onNext.Should().Equal([10, 20, 30]);
        onErrorResume.Select(x => x.Message).Should().Equal(["a", "b", "c"]);


        disposeCalled.Should().BeFalse();

        subject.OnCompleted();
        onCompleted.Should().ContainSingle();

        disposeCalled.Should().BeTrue();
    }
}
