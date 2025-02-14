namespace R3.Tests.OperatorTests;

public class MaterializeTest
{
    [Fact]
    public void Materialize()
    {
        var publisher = new Subject<int>();
        var list = publisher.Materialize().ToLiveList();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);
        publisher.OnErrorResume(new Exception("foo"));
        publisher.OnCompleted(new Exception("comp"));

        list[0].Value.ShouldBe(10);
        list[1].Value.ShouldBe(20);
        list[2].Value.ShouldBe(30);
        list[3].Error!.Message.ShouldBe("foo");
        list[4].Result!.Exception!.Message.ShouldBe("comp");

        list.AssertIsCompleted();
    }

    [Fact]
    public void Dematerialize()
    {
        var publisher = new Subject<int>();
        var list = publisher.Materialize().Dematerialize().ToLiveList();

        publisher.OnNext(10);
        publisher.OnNext(20);
        publisher.OnNext(30);
        publisher.OnErrorResume(new Exception("foo"));
        publisher.OnCompleted(new Exception("comp"));

        list.AssertEqual([10, 20, 30]);
        list.Result.IsFailure.ShouldBeTrue();
        list.AssertIsCompleted();
    }
}
