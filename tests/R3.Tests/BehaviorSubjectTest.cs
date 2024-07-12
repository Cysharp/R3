namespace R3.Tests;

public class BehaviorSubjectTest
{
    [Fact]
    public void Test()
    {
        // Dispose(not yet completed)
        {
            var s = new BehaviorSubject<int>(100);
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.Dispose();

            l.AssertEqual([100, 1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }

        // already OnCompleted(Success), Dispose
        {
            var s = new BehaviorSubject<int>(100);
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.OnCompleted();
            s.Dispose();

            l.AssertEqual([100, 1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }

        // already OnCompleted(Failure), Dispose
        {
            var s = new BehaviorSubject<int>(100);
            using var l = s.ToLiveList();
            s.OnNext(1);
            s.OnNext(2);
            s.OnNext(3);
            s.OnCompleted(new Exception("foo"));
            s.Dispose();

            l.AssertEqual([100, 1, 2, 3]);
            l.AssertIsCompleted();
            s.IsDisposed.Should().BeTrue();
        }


        // already Disposed, call OnNext
        {
            var s = new BehaviorSubject<int>(100);
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnNext(1));
        }
        // already Disposed, call OnError
        {
            var s = new BehaviorSubject<int>(100);
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnErrorResume(new Exception()));
        }
        // already Disposed, call OnCompleted
        {
            var s = new BehaviorSubject<int>(100);
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => s.OnCompleted());
        }
        // already Disposed, call Value
        {
            var s = new BehaviorSubject<int>(100);
            s.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _ = s.Value);
        }
    }

    [Fact]
    public void SubscribeAfterCompleted()
    {
        {
            // after Success
            var s = new BehaviorSubject<int>(100);
            s.OnCompleted();

            using var l = s.ToLiveList();

            l.AssertIsCompleted();
            l.Result.IsSuccess.Should().BeTrue();
            l.Count.Should().Be(0); // doesnt publish on subscribe

            // get value is ok, latest
            s.Value.Should().Be(100);
        }
        {
            // after Failure
            var s = new BehaviorSubject<int>(100);
            s.OnCompleted(new Exception("foo"));

            using var l = s.ToLiveList();

            l.AssertIsCompleted();
            l.Result.IsFailure.Should().BeTrue();
            l.Result.Exception!.Message.Should().Be("foo");

            Assert.Throws<Exception>(() => _ = s.Value).Message.Should().Be("foo");
        }
    }
}
