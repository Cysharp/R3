namespace R3.Tests;

public class DisposableBuilderTest
{
    // 1~8 = Combined, 9~ array
    [Fact]
    public void Combined()
    {
        // combined check
        for (int i = 1; i <= 8; i++)
        {
            var l = new List<int>();
            using var builder = Disposable.CreateBuilder();
            for (int j = 0; j < i; j++)
            {
                builder.Add(Disposable.Create(() => l.Add(j + 1)));
            }

            var disposable = builder.Build();

            if (i == 1)
            {
                disposable.GetType().Name.Should().Be("AnonymousDisposable");
            }
            else
            {
                disposable.GetType().Name.Should().StartWith("CombinedDisposable");
            }

            l.Should().BeEmpty();

            disposable.Dispose();

            l.Should().HaveCount(i);
        }
    }

    [Fact]
    public void Array()
    {
        var l = new List<int>();
        using var builder = Disposable.CreateBuilder();
        for (int i = 1; i <= 8; i++)
        {
            var v = i;
            builder.Add(Disposable.Create(() => l.Add(v)));
        }

        // array
        builder.Add(Disposable.Create(() => l.Add(9)));
        builder.Add(Disposable.Create(() => l.Add(10)));
        builder.Add(Disposable.Create(() => l.Add(11)));
        builder.Add(Disposable.Create(() => l.Add(12)));
        builder.Add(Disposable.Create(() => l.Add(13)));
        builder.Add(Disposable.Create(() => l.Add(14)));
        builder.Add(Disposable.Create(() => l.Add(15)));
        builder.Add(Disposable.Create(() => l.Add(16)));
        // grow
        builder.Add(Disposable.Create(() => l.Add(17)));
        builder.Add(Disposable.Create(() => l.Add(18)));
        builder.Add(Disposable.Create(() => l.Add(19)));
        builder.Add(Disposable.Create(() => l.Add(20)));

        var disposable = builder.Build();

        disposable.GetType().Name.Should().Be("CombinedDisposable");

        l.Should().BeEmpty();

        disposable.Dispose();

        l.Should().Equal([
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            20]);
    }

}
