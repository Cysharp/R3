using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace R3.Tests.OperatorTests;

public class SumTest
{
    [Fact]
    public async Task Empty()
    {
        (await Observable.Empty<int>().SumAsync()).Should().Be(0);
        (await Observable.Empty<TestNumber>().SumAsync()).Value.Should().Be(0);
    }

    [Fact]
    public async Task One()
    {
        (await Observable.Return(999).SumAsync()).Should().Be(999);
        (await Observable.Return(new TestNumber(999)).SumAsync()).Value.Should().Be(999);
    }

    [Fact]
    public async Task Error()
    {
        var error = Observable.Range(0, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.SumAsync());
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().SumAsync());
    }

    [Fact]
    public async Task MultipleValues()
    {
        var source = new int[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();
        (await source.SumAsync()).Should().Be(36);

        var source2 = source.Select(x => new TestNumber(x));
        (await source2.SumAsync()).Value.Should().Be(36);
    }

    [Fact]
    public async Task WithSelector()
    {
        var source = new[] { 1, 10, 1, 3, 4, 6, 7, 4 }.ToObservable();

        (await source.SumAsync(x => x * 10f)).Should().Be(360f);
        // (await source.SumAsync(x => new TestNumber(x))).Value.Should().Be(36);
    }

    [Fact]
    public async Task WithSelectorError()
    {
        var error = Observable.Range(1, 10).Select(x =>
        {
            if (x == 3) throw new Exception("foo");
            return x;
        });

        await Assert.ThrowsAsync<Exception>(async () => await error.SumAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await error.OnErrorResumeAsFailure().SumAsync(x => x));
        await Assert.ThrowsAsync<Exception>(async () => await Observable.Range(0, 10).SumAsync(x => throw new Exception("bra")));
    }

    record struct TestNumber(int Value) : INumber<TestNumber>
    {
        public static TestNumber One => throw new NotImplementedException();

        public static int Radix => throw new NotImplementedException();

        public static TestNumber Zero => new TestNumber(0);

        public static TestNumber AdditiveIdentity => throw new NotImplementedException();

        public static TestNumber MultiplicativeIdentity => throw new NotImplementedException();

        static TestNumber INumberBase<TestNumber>.One => throw new NotImplementedException();

        static int INumberBase<TestNumber>.Radix => throw new NotImplementedException();

        static TestNumber INumberBase<TestNumber>.Zero => new TestNumber(0);

        static TestNumber IAdditiveIdentity<TestNumber, TestNumber>.AdditiveIdentity => throw new NotImplementedException();

        static TestNumber IMultiplicativeIdentity<TestNumber, TestNumber>.MultiplicativeIdentity => throw new NotImplementedException();

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

        public static TestNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static TestNumber Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out TestNumber result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out TestNumber result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out TestNumber result)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.Abs(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsCanonical(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsComplexNumber(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsEvenInteger(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsFinite(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsImaginaryNumber(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsInfinity(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsInteger(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsNaN(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsNegative(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsNegativeInfinity(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsNormal(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsOddInteger(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsPositive(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsPositiveInfinity(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsRealNumber(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsSubnormal(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.IsZero(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.MaxMagnitude(TestNumber x, TestNumber y)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.MaxMagnitudeNumber(TestNumber x, TestNumber y)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.MinMagnitude(TestNumber x, TestNumber y)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.MinMagnitudeNumber(TestNumber x, TestNumber y)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        static TestNumber INumberBase<TestNumber>.Parse(string s, NumberStyles style, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        static TestNumber ISpanParsable<TestNumber>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        static TestNumber IParsable<TestNumber>.Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertFromChecked<TOther>(TOther value, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertFromSaturating<TOther>(TOther value, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertFromTruncating<TOther>(TOther value, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertToChecked<TOther>(TestNumber value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertToSaturating<TOther>(TestNumber value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryConvertToTruncating<TOther>(TestNumber value, out TOther result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool INumberBase<TestNumber>.TryParse(string? s, NumberStyles style, IFormatProvider? provider, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool ISpanParsable<TestNumber>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        static bool IParsable<TestNumber>.TryParse(string? s, IFormatProvider? provider, out TestNumber result)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(TestNumber other)
        {
            throw new NotImplementedException();
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        int IComparable.CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        int IComparable<TestNumber>.CompareTo(TestNumber other)
        {
            throw new NotImplementedException();
        }

        bool IEquatable<TestNumber>.Equals(TestNumber other)
        {
            throw new NotImplementedException();
        }

        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            throw new NotImplementedException();
        }

        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator +(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static TestNumber IUnaryPlusOperators<TestNumber, TestNumber>.operator +(TestNumber value)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator +(TestNumber left, TestNumber right) =>
            new(left.Value + right.Value);

        static TestNumber IAdditionOperators<TestNumber, TestNumber, TestNumber>.operator +(TestNumber left, TestNumber right)
        {
            return new(left.Value + right.Value);
        }

        public static TestNumber operator -(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static TestNumber IUnaryNegationOperators<TestNumber, TestNumber>.operator -(TestNumber value)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator -(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static TestNumber ISubtractionOperators<TestNumber, TestNumber, TestNumber>.operator -(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator ++(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static TestNumber IIncrementOperators<TestNumber>.operator ++(TestNumber value)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator --(TestNumber value)
        {
            throw new NotImplementedException();
        }

        static TestNumber IDecrementOperators<TestNumber>.operator --(TestNumber value)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator *(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static TestNumber IMultiplyOperators<TestNumber, TestNumber, TestNumber>.operator *(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator /(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static TestNumber IDivisionOperators<TestNumber, TestNumber, TestNumber>.operator /(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static TestNumber operator %(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static TestNumber IModulusOperators<TestNumber, TestNumber, TestNumber>.operator %(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IEqualityOperators<TestNumber, TestNumber, bool>.operator ==(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IEqualityOperators<TestNumber, TestNumber, bool>.operator !=(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static bool operator <(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IComparisonOperators<TestNumber, TestNumber, bool>.operator <(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static bool operator >(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IComparisonOperators<TestNumber, TestNumber, bool>.operator >(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static bool operator <=(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IComparisonOperators<TestNumber, TestNumber, bool>.operator <=(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        public static bool operator >=(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }

        static bool IComparisonOperators<TestNumber, TestNumber, bool>.operator >=(TestNumber left, TestNumber right)
        {
            throw new NotImplementedException();
        }
    }
}
