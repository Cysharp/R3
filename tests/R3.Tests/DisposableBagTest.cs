using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests;

public class DisposableBagTest
{
    [Fact]
    public void Test()
    {
        var bag = new DisposableBag();

        var disposed = new bool[6];

        var subject = new Subject<int>();
        subject.Do(onDispose: () => disposed[0] = true).Subscribe().AddTo(ref bag);
        subject.Do(onDispose: () => disposed[1] = true).Subscribe().AddTo(ref bag);
        subject.Do(onDispose: () => disposed[2] = true).Subscribe().AddTo(ref bag);
        subject.Do(onDispose: () => disposed[3] = true).Subscribe().AddTo(ref bag);
        subject.Do(onDispose: () => disposed[4] = true).Subscribe().AddTo(ref bag);
        subject.Do(onDispose: () => disposed[5] = true).Subscribe().AddTo(ref bag);

        disposed.Should().Equal([false, false, false, false, false, false]);

        bag.Dispose();

        disposed.Should().Equal([true, true, true, true, true, true]);
    }
}
