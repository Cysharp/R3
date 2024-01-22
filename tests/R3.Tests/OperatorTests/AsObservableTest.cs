namespace R3.Tests.OperatorTests;

public class AsObservableTest
{
    [Fact]
    public void AsObservable()
    {
        var p = new Subject<int>();

        var l = p.AsObservable().AsObservable().ToLiveList();
        p.OnNext(1);
        p.OnNext(2);
        p.OnNext(3);
        p.OnCompleted();

        l.AssertEqual([1, 2, 3]);
        l.AssertIsCompleted();
    }

    [Fact]
    public void AsSystemObservable()
    {

        {
            var p = new Subject<int>();
            var l = new List<int>();
            Exception? ex = null;
            bool completed = false;
            p.AsSystemObservable().Subscribe(l.Add, e => ex = e, () => completed = true);

            p.OnNext(1);
            p.OnNext(2);
            p.OnNext(3);
            p.OnCompleted();

            l.Should().Equal([1, 2, 3]);
            completed.Should().BeTrue();
        }
        {
            // error complete
            var p = new Subject<int>();
            var l = new List<int>();
            Exception? ex = null;
            bool completed = false;
            p.AsSystemObservable().Subscribe(l.Add, e => ex = e, () => completed = true);

            p.OnNext(1);
            p.OnNext(2);
            p.OnNext(3);
            p.OnCompleted(new Exception("aaa"));

            l.Should().Equal([1, 2, 3]);
            ex!.Message.Should().Be("aaa");
            completed.Should().BeFalse();
        }
        {
            // error resume
            var p = new Subject<int>();
            var l = new List<int>();
            Exception? ex = null;
            bool completed = false;
            p.AsSystemObservable().Subscribe(l.Add, e => ex = e, () => completed = true);

            p.OnNext(1);
            p.OnNext(2);
            p.OnNext(3);
            p.OnErrorResume(new Exception("bbb"));

            l.Should().Equal([1, 2, 3]);
            ex!.Message.Should().Be("bbb");
            completed.Should().BeFalse();
        }
    }
}
