namespace R3.Tests.FactoryTests;

public class FromEventTest
{

    [Fact]
    public void Event()
    {
        var ev = new EventPattern();

        var l1 = Observable.FromEventHandler(h => ev.E1 += h, h => ev.E1 -= h).ToLiveList();
        var l2 = Observable.FromEventHandler<int>(h => ev.E2 += h, h => ev.E2 -= h).ToLiveList();
        var l3 = Observable.FromEvent(h => ev.A1 += h, h => ev.A1 -= h).ToLiveList();
        var l4 = Observable.FromEvent<int>(h => ev.A2 += h, h => ev.A2 -= h).ToLiveList();
        var l5 = Observable.FromEvent<MyDelegate1>(h => new MyDelegate1(h), h => ev.M1 += h, h => ev.M1 -= h).ToLiveList();
        var l6 = Observable.FromEvent<MyDelegate2, int>(h => new MyDelegate2(h), h => ev.M2 += h, h => ev.M2 -= h).ToLiveList();
        var l7 = Observable.FromEvent<MyDelegate3, (int x, int y)>(h => (x, y) => h((x, y)), h => ev.M3 += h, h => ev.M3 -= h).ToLiveList();

        ev.Raise(10, 20);
        ev.Raise(100, 200);

        l1.Should().HaveCount(2);
        l3.Should().HaveCount(2);
        l5.Should().HaveCount(2);

        l2.Select(x => x.e).Should().Equal([10, 100]);
        l4.AssertEqual([10, 100]);
        l6.AssertEqual([10, 100]);
        l7.AssertEqual([(10, 20), (100, 200)]);

        ev.InvocationListCount().Should().Be((1, 1, 1, 1, 1, 1, 1));

        l1.Dispose();
        l2.Dispose();
        l3.Dispose();
        l4.Dispose();
        l5.Dispose();
        l6.Dispose();
        l7.Dispose();

        ev.InvocationListCount().Should().Be((0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Cancel()
    {
        var cts = new CancellationTokenSource();

        var ev = new EventPattern();

        var l1 = Observable.FromEventHandler(h => ev.E1 += h, h => ev.E1 -= h, cts.Token).ToLiveList();
        var l2 = Observable.FromEventHandler<int>(h => ev.E2 += h, h => ev.E2 -= h, cts.Token).ToLiveList();
        var l3 = Observable.FromEvent(h => ev.A1 += h, h => ev.A1 -= h, cts.Token).ToLiveList();
        var l4 = Observable.FromEvent<int>(h => ev.A2 += h, h => ev.A2 -= h, cts.Token).ToLiveList();
        var l5 = Observable.FromEvent<MyDelegate1>(h => new MyDelegate1(h), h => ev.M1 += h, h => ev.M1 -= h, cts.Token).ToLiveList();
        var l6 = Observable.FromEvent<MyDelegate2, int>(h => new MyDelegate2(h), h => ev.M2 += h, h => ev.M2 -= h, cts.Token).ToLiveList();
        var l7 = Observable.FromEvent<MyDelegate3, (int x, int y)>(h => (x, y) => h((x, y)), h => ev.M3 += h, h => ev.M3 -= h, cts.Token).ToLiveList();

        ev.Raise(10, 20);
        ev.Raise(100, 200);

        l1.Should().HaveCount(2);
        l3.Should().HaveCount(2);
        l5.Should().HaveCount(2);

        l2.Select(x => x.e).Should().Equal([10, 100]);
        l4.AssertEqual([10, 100]);
        l6.AssertEqual([10, 100]);
        l7.AssertEqual([(10, 20), (100, 200)]);

        ev.InvocationListCount().Should().Be((1, 1, 1, 1, 1, 1, 1));

        cts.Cancel();

        ev.InvocationListCount().Should().Be((0, 0, 0, 0, 0, 0, 0));
    }


    class EventPattern
    {
        public event EventHandler? E1;
        public event EventHandler<int>? E2;
        public event Action? A1;
        public event Action<int>? A2;
        public event MyDelegate1? M1;
        public event MyDelegate2? M2;
        public event MyDelegate3? M3;

        public void Raise(int x, int y)
        {
            E1?.Invoke(this, new EventArgs());
            E2?.Invoke(this, x);
            A1?.Invoke();
            A2?.Invoke(x);
            M1?.Invoke();
            M2?.Invoke(x);
            M3?.Invoke(x, y);
        }

        public (int, int, int, int, int, int, int) InvocationListCount()
        {
            return (
                E1?.GetInvocationList().Length ?? 0,
                E2?.GetInvocationList().Length ?? 0,
                A1?.GetInvocationList().Length ?? 0,
                A2?.GetInvocationList().Length ?? 0,
                M1?.GetInvocationList().Length ?? 0,
                M2?.GetInvocationList().Length ?? 0,
                M3?.GetInvocationList().Length ?? 0);
        }
    }

    public delegate void MyDelegate1();
    public delegate void MyDelegate2(int x);
    public delegate void MyDelegate3(int x, int y);
}
