namespace R3.Tests.OperatorTests;

public class CatchTest
{
    [Fact]
    public void CatchSecond()
    {
        {
            var first = new Subject<int>();
            var second = new Subject<int>();

            using var list = first.Catch(second).ToLiveList();

            first.OnNext(10);
            first.OnNext(20);
            first.OnNext(30);
            second.OnNext(9999);
            first.OnCompleted();
            second.OnNext(9998);

            list.AssertEqual([10, 20, 30]);
            list.AssertIsCompleted();
        }
        {
            var first = new Subject<int>();
            var second = new Subject<int>();

            using var list = first.Catch(second).ToLiveList();

            first.OnNext(10);
            first.OnNext(20);
            first.OnNext(30);
            second.OnNext(9999);
            first.OnCompleted(new Exception()); // error, so switch to second
            list.AssertIsNotCompleted();
            second.OnNext(9998);

            list.AssertEqual([10, 20, 30, 9998]);
            second.OnCompleted();
            list.AssertIsCompleted();
        }
    }

    [Fact]
    public void CatchHandler()
    {
        {
            var first = new Subject<int>();
            var second = new Subject<int>();

            using var list = first.Catch<int, ArgumentException>(ex => second).ToLiveList();

            first.OnNext(10);
            first.OnNext(20);
            first.OnNext(30);
            second.OnNext(9999);
            first.OnCompleted();
            second.OnNext(9998);

            list.AssertEqual([10, 20, 30]);
            list.AssertIsCompleted();
        }
        {
            var first = new Subject<int>();
            var second = new Subject<int>();

            using var list = first.Catch<int, ArgumentException>(ex => second).ToLiveList();

            first.OnNext(10);
            first.OnNext(20);
            first.OnNext(30);
            second.OnNext(9999);
            first.OnCompleted(new Exception()); // error, but not switch
            list.AssertEqual([10, 20, 30]);
            list.AssertIsCompleted();
        }
        {
            var first = new Subject<int>();
            var second = new Subject<int>();

            using var list = first.Catch<int, ArgumentException>(ex => second).ToLiveList();

            first.OnNext(10);
            first.OnNext(20);
            first.OnNext(30);
            second.OnNext(9999);
            first.OnCompleted(new ArgumentException()); // error, switch
            list.AssertIsNotCompleted();
            second.OnNext(9998);

            list.AssertEqual([10, 20, 30, 9998]);
            second.OnCompleted();
            list.AssertIsCompleted();
        }
    }
}
