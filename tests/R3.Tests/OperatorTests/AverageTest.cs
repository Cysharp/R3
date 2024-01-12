using System.Globalization;
using System.Numerics;

namespace R3.Tests.OperatorTests;

public class AverageTest
{
    [Fact]
    public async Task Empty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Observable.Empty<int>().AverageAsync();
        });
        //await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //{
        //    await Observable.Empty<TestNumber>().AverageAsync();
        //});

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Observable.Empty<int>().AverageAsync(x => x);
        });
        //await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //{
        //    await Observable.Empty<TestNumber>().AverageAsync(x => x);
        //});
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(7).AverageAsync()).Should().Be(7.0);
        (await Observable.Return(7).AverageAsync(x => x * 10)).Should().Be(70.0);
        //(await Observable.Return(new TestNumber(7)).AverageAsync()).Should().Be(7.0);
        //(await Observable.Return(new TestNumber(7)).AverageAsync(x => x)).Should().Be(7.0);
    }

    [Fact]
    public async Task AnyValues()
    {
        var values = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 };

        var result = await values.ToObservable().AverageAsync();
        result.Should().Be(values.Average());

        result = await values.ToObservable().AverageAsync(x => x * 2);
        result.Should().Be(values.Average(x => x * 2));
    }

    [Fact]
    public async Task Error()
    {
        var error = Observable.Range(0, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.AverageAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().AverageAsync());
    }

    [Fact]
    public async Task SelectorError()
    {
        var o = Observable.Range(1, 10);

        await Assert.ThrowsAsync<Exception>(async () => await o.AverageAsync(_ => throw new Exception("foo")));
        await Assert.ThrowsAsync<Exception>(async () => await o.Select(x => new TestNumber(x)).AverageAsync(x => throw new Exception("bra")));
    }

    //[Fact]
    //public async Task DoubleConvertError()
    //{
    //    var o = Observable.Return(new TestNumber(100, CannotConvert: true));

    //    await Assert.ThrowsAsync<NotSupportedException>(async () =>
    //    {
    //        await o.AverageAsync();
    //    });
    //    await Assert.ThrowsAsync<NotSupportedException>(async () =>
    //    {
    //        await o.AverageAsync(x => x);
    //    });
    //}
}

file record struct TestNumber(int Value, bool CannotConvert = false) : INumberBase<TestNumber>
{
    public static TestNumber operator +(TestNumber left, TestNumber right) =>
        new(left.Value + right.Value, right.CannotConvert);

    public static TestNumber operator /(TestNumber left, TestNumber right) =>
        new(left.Value / right.Value, right.CannotConvert);

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        throw new NotImplementedException();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static TestNumber Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out TestNumber result)
    {
        throw new NotImplementedException();
    }

    public static TestNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out TestNumber result)
    {
        throw new NotImplementedException();
    }

    public static TestNumber AdditiveIdentity => throw new NotImplementedException();

    public static TestNumber operator --(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static TestNumber operator ++(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static TestNumber MultiplicativeIdentity => throw new NotImplementedException();
    public static TestNumber operator *(TestNumber left, TestNumber right)
    {
        throw new NotImplementedException();
    }

    public static TestNumber operator -(TestNumber left, TestNumber right)
    {
        throw new NotImplementedException();
    }

    public static TestNumber operator -(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static TestNumber operator +(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static TestNumber Abs(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsCanonical(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsComplexNumber(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsEvenInteger(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsFinite(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsImaginaryNumber(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsInfinity(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsInteger(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNaN(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNegative(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNegativeInfinity(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNormal(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsOddInteger(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositive(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositiveInfinity(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsRealNumber(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsSubnormal(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(TestNumber value)
    {
        throw new NotImplementedException();
    }

    public static TestNumber MaxMagnitude(TestNumber x, TestNumber y)
    {
        throw new NotImplementedException();
    }

    public static TestNumber MaxMagnitudeNumber(TestNumber x, TestNumber y)
    {
        throw new NotImplementedException();
    }

    public static TestNumber MinMagnitude(TestNumber x, TestNumber y)
    {
        throw new NotImplementedException();
    }

    public static TestNumber MinMagnitudeNumber(TestNumber x, TestNumber y)
    {
        throw new NotImplementedException();
    }

    public static TestNumber Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static TestNumber Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out TestNumber result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out TestNumber result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out TestNumber result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(TestNumber value, out TOther result) where TOther : INumberBase<TOther>
    {
        if (value.CannotConvert)
        {
            throw new NotSupportedException();
        }

        if (typeof(TOther) == typeof(double))
        {
            result = (TOther)(object)(double)value.Value;
            return true;
        }
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(TestNumber value, out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(TestNumber value, out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out TestNumber result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out TestNumber result)
    {
        throw new NotImplementedException();
    }

    public static TestNumber One => throw new NotImplementedException();
    public static int Radix => throw new NotImplementedException();
    public static TestNumber Zero => throw new NotImplementedException();
}
