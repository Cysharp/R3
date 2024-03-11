using R3.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace R3.Tests;

public class ReactivePropertyTest
{
    [Fact]
    public void Test()
    {
        var rp = new ReactiveProperty<int>(100);
        rp.Value.Should().Be(100);

        var list = rp.ToLiveList();
        list.AssertEqual([100]);

        rp.Value = 9999;

        var list2 = rp.ToLiveList();
        list.AssertEqual([100, 9999]);
        list2.AssertEqual([9999]);

        rp.Value = 9999;
        list.AssertEqual([100, 9999]);
        list2.AssertEqual([9999]);

        rp.Value = 100;
        list.AssertEqual([100, 9999, 100]);
        list2.AssertEqual([9999, 100]);

        rp.Dispose();

        list.AssertIsCompleted();
        list2.AssertIsCompleted();
    }

    [Fact]
    public void DefaultValueTest()
    {
        using var rp = new ReactiveProperty<int>();
        rp.Value.Should().Be(default);
    }

    [Fact]
    public void SubscribeAfterCompleted()
    {
        var rp = new ReactiveProperty<string>("foo");
        rp.OnCompleted();

        using var list = rp.ToLiveList();

        list.AssertIsCompleted();
        list.AssertEqual(["foo"]);
    }

    // Node Check
    [Fact]
    public void CheckNode()
    {
        var rp = new ReactiveProperty<int>();


        var list1 = rp.ToLiveList();

        rp.Value = 1;

        list1.AssertEqual([0, 1]);

        list1.Dispose();

        var list2 = rp.ToLiveList();

        rp.Value = 2;

        list2.AssertEqual([1, 2]);

        var list3 = rp.ToLiveList();
        var list4 = rp.ToLiveList();

        // remove first

        list2.Dispose();
        rp.Value = 3;

        list3.AssertEqual([2, 3]);
        list4.AssertEqual([2, 3]);

        var list5 = rp.ToLiveList();

        // remove middle
        list4.Dispose();

        rp.Value = 4;

        list3.AssertEqual([2, 3, 4]);
        list5.AssertEqual([3, 4]);

        // remove last
        list5.Dispose();

        rp.Value = 5;
        list3.AssertEqual([2, 3, 4, 5]);

        // remove single
        list3.Dispose();

        // subscribe once
        var list6 = rp.ToLiveList();
        rp.Value = 6;
        list6.AssertEqual([5, 6]);
    }

    [Fact]
    public void NodeDisposeCheck()
    {
        {
            var rp = new ReactiveProperty<int>(1);

            LiveList<int> list1;
            LiveList<int> list2;
            LiveList<int> list3 = null!;
            LiveList<int> list4 = null!;
            LiveList<int> list5;

            list1 = rp.ToLiveList();
            list2 = rp.Take(2).Do(_ => { list3?.Dispose(); list4?.Dispose(); }).ToLiveList();
            list3 = rp.ToLiveList();
            list4 = rp.ToLiveList();
            list5 = rp.ToLiveList();

            rp.Value = 10;

            list1.AssertEqual([1, 10]);
            list2.AssertEqual([1, 10]);
            list3.AssertEqual([1]);
            list4.AssertEqual([1]);
            list5.AssertEqual([1, 10]);
        }
        {
            // Dispose only self.
            var rp = new ReactiveProperty<int>(1);

            LiveList<int> list1;
            LiveList<int> list2;
            LiveList<int> list3 = null!;
            LiveList<int> list4 = null!;
            LiveList<int> list5;

            list1 = rp.ToLiveList();
            list2 = rp.Take(2).Do(_ => { /* list3?.Dispose(); list4?.Dispose(); */}).ToLiveList();
            list3 = rp.ToLiveList();
            list4 = rp.ToLiveList();
            list5 = rp.ToLiveList();

            rp.Value = 10;

            list1.AssertEqual([1, 10]);
            list2.AssertEqual([1, 10]);
            list3.AssertEqual([1, 10]);
            list4.AssertEqual([1, 10]);
            list5.AssertEqual([1, 10]);

            rp.Value = 20;

            list1.AssertEqual([1, 10, 20]);
            list2.AssertEqual([1, 10]);
            list3.AssertEqual([1, 10, 20]);
            list4.AssertEqual([1, 10, 20]);
            list5.AssertEqual([1, 10, 20]);
        }
        {
            // Dispose only next one.
            var rp = new ReactiveProperty<int>(1);

            LiveList<int> list1;
            LiveList<int> list2;
            LiveList<int> list3 = null!;
            LiveList<int> list4 = null!;
            LiveList<int> list5;

            list1 = rp.ToLiveList();
            list2 = rp.Do(_ => { list3?.Dispose(); /* list4?.Dispose(); */}).ToLiveList();
            list3 = rp.ToLiveList();
            list4 = rp.ToLiveList();
            list5 = rp.ToLiveList();

            rp.Value = 10;

            list1.AssertEqual([1, 10]);
            list2.AssertEqual([1, 10]);
            list3.AssertEqual([1]);
            list4.AssertEqual([1, 10]);
            list5.AssertEqual([1, 10]);

            rp.Value = 20;

            list1.AssertEqual([1, 10, 20]);
            list2.AssertEqual([1, 10, 20]);
            list3.AssertEqual([1]);
            list4.AssertEqual([1, 10, 20]);
            list5.AssertEqual([1, 10, 20]);
        }
        {
            // Dispose only next next one.
            var rp = new ReactiveProperty<int>(1);

            LiveList<int> list1;
            LiveList<int> list2;
            LiveList<int> list3 = null!;
            LiveList<int> list4 = null!;
            LiveList<int> list5;

            list1 = rp.ToLiveList();
            list2 = rp.Do(_ => { list4?.Dispose(); }).ToLiveList();
            list3 = rp.ToLiveList();
            list4 = rp.ToLiveList();
            list5 = rp.ToLiveList();

            rp.Value = 10;

            list1.AssertEqual([1, 10]);
            list2.AssertEqual([1, 10]);
            list3.AssertEqual([1, 10]);
            list4.AssertEqual([1]);
            list5.AssertEqual([1, 10]);

            rp.Value = 20;

            list1.AssertEqual([1, 10, 20]);
            list2.AssertEqual([1, 10, 20]);
            list3.AssertEqual([1, 10, 20]);
            list4.AssertEqual([1]);
            list5.AssertEqual([1, 10, 20]);
        }
    }

    [Fact]
    public void RecursiveSubscribe()
    {
        var rp = new ReactiveProperty<int>(0);

        List<LiveList<int>> recList = new();

        var list = rp.Do(x =>
            {
                recList.Add(rp.ToLiveList());
            })
            .ToLiveList();

        list.AssertEqual([0]);
        recList[0].AssertEqual([0]);

        rp.Value = 99;
        list.AssertEqual([0, 99]);
        recList[0].AssertEqual([0, 99]);
        recList[1].AssertEqual([99]);


    }

    [Fact]
    public void RootChangedFromSecond()
    {

        var p1 = new ReactiveProperty<int>();
        var p1List = p1.Skip(1).ToLiveList();

        p1.Skip(1).Subscribe().Dispose(); // Subscribe and Dispose

        var p3List = p1.Skip(1).ToLiveList();

        p1.Value = 1;
        p1.Value = 2;

        p1List.AssertEqual([1, 2]);
        p3List.AssertEqual([1, 2]);
    }

    [Fact]
    public void RemoveLastNode()
    {
        var log = new List<string>();

        var count = 0;
        var r = new ReactiveProperty<int>(count);
        var ctsA = new CancellationTokenSource();
        var ctsB = new CancellationTokenSource();
        var ctsC = new CancellationTokenSource();
        r.Subscribe(x => log.Add($"A = {x}")).RegisterTo(ctsA.Token);
        r.Subscribe(x => log.Add($"B = {x}")).RegisterTo(ctsB.Token);
        r.Subscribe(x => log.Add($"C = {x}")).RegisterTo(ctsC.Token);
        ctsA.Cancel();
        ctsA.Dispose();
        log.Add("A disposed");

        ctsA = new CancellationTokenSource();
        r.Subscribe(x => log.Add($"A = {x}")).RegisterTo(ctsA.Token);
        log.Add("A re-registered");
        ctsA.Cancel();
        ctsA.Dispose();
        log.Add("A disposed");
        ctsA = new CancellationTokenSource();
        r.Subscribe(x => log.Add($"A = {x}")).RegisterTo(ctsA.Token);
        log.Add("A re-registered");
        r.Value = ++count;
        r.Value = ++count;

        var actual = string.Join(Environment.NewLine, log);

        actual.Should().Be("""
A = 0
B = 0
C = 0
A disposed
A = 0
A re-registered
A disposed
A = 0
A re-registered
B = 1
C = 1
A = 1
B = 2
C = 2
A = 2
""");
    }

    [Fact]
    public void RemoveMiddle()
    {
        var log = new List<string>();

        var p1 = new ReactiveProperty<int>();
        p1.Skip(1).Subscribe(x => log.Add("[P1]" + x)); // alive

        var d1 = p1.Skip(1).Subscribe(x => log.Add("[P2]" + x));
        var d2 = p1.Skip(1).Subscribe(x => log.Add("[P2]" + x));
        d1.Dispose();
        d2.Dispose();

        p1.Skip(1).Subscribe(x => log.Add("[P3]" + x)); // alive

        p1.Value = 1;
        p1.Value = 2;

        var actual = string.Join(Environment.NewLine, log);

        actual.Should().Be("""
[P1]1
[P3]1
[P1]2
[P3]2
""");
    }



    [Fact]
    public void RemoveFirst()
    {
        var log = new List<string>();

        var p1 = new ReactiveProperty<int>();
        var d1 = p1.Skip(1).Subscribe(x => log.Add("[P1]" + x));

        var d2 = p1.Skip(1).Subscribe(x => log.Add("[P2_1]" + x)); // alive
        var d3 = p1.Skip(1).Subscribe(x => log.Add("[P2_2]" + x)); // alive
        
        d1.Dispose();

        p1.Skip(1).Subscribe(x => log.Add("[P3]" + x)); // alive

        p1.Value = 1;
        p1.Value = 2;

        var actual = string.Join(Environment.NewLine, log);

        actual.Should().Be("""
[P2_1]1
[P2_2]1
[P3]1
[P2_1]2
[P2_2]2
[P3]2
""");
    }

    [Fact]
    public void RemoveLast()
    {
        var log = new List<string>();

        var p1 = new ReactiveProperty<int>();
        p1.Skip(1).Subscribe(x => log.Add("[P1]" + x)); // alive

        var d1 = p1.Skip(1).Subscribe(x => log.Add("[P2]" + x)); // alive
        var d2 = p1.Skip(1).Subscribe(x => log.Add("[P2]" + x)); // alive

        var d3 = p1.Skip(1).Subscribe(x => log.Add("[P3]" + x));
        d3.Dispose();

        p1.Value = 1;
        p1.Value = 2;

        var actual = string.Join(Environment.NewLine, log);

        actual.Should().Be("""
[P1]1
[P2]1
[P2]1
[P1]2
[P2]2
[P2]2
""");
    }
}
