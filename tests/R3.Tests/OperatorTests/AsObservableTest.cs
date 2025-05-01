﻿namespace R3.Tests.OperatorTests;

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
    public void AsObservableWithDelay()
    {
        var p = new Subject<int>();
        var fakeFrameProvider = new FakeFrameProvider();

        var l = p.AsObservable().DelayFrame(1, fakeFrameProvider).ToLiveList();
        p.OnNext(1);
        p.OnNext(2);
        p.OnNext(3);
        p.OnCompleted();
        fakeFrameProvider.Advance();

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

            l.ShouldBe([1, 2, 3]);
            completed.ShouldBeTrue();
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

            l.ShouldBe([1, 2, 3]);
            ex!.Message.ShouldBe("aaa");
            completed.ShouldBeFalse();
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

            l.ShouldBe([1, 2, 3]);
            ex!.Message.ShouldBe("bbb");
            completed.ShouldBeFalse();
        }
    }
}
