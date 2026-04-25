using Raffinert.FuzzySharp.Utils;
using System;

namespace Raffinert.FuzzySharp;

public sealed partial class Indel(string source) : IDisposable
{
    private readonly string _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly IPatternMatchVector<char> _patternMatchVector = PatternMatchVector.Create(source.AsSpan());

    public int DistanceFrom(string value)
    {
        return DistanceImpl(_source.AsSpan(), value.AsSpan(), _patternMatchVector);
    }

    public double NormalizedSimilarityWith(string value)
    {
        return NormalizedSimilarityImpl(_source.AsSpan(), value.AsSpan(), _patternMatchVector);
    }

    public void Dispose()
    {
        _patternMatchVector.Dispose();
    }
}

public sealed class IndelT<T>(T[] source) : IDisposable where T : IEquatable<T>
{
    private readonly T[] _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly IPatternMatchVector<T> _patternMatchVector = PatternMatchVector.Create<T>(source.AsSpan());

    public int DistanceFrom(T[] value)
    {
        return Indel.DistanceImpl<T>(_source.AsSpan(), value.AsSpan(), _patternMatchVector);
    }

    public double NormalizedSimilarityWith(T[] value)
    {
        return Indel.NormalizedSimilarityImpl<T>(_source.AsSpan(), value.AsSpan(), _patternMatchVector);
    }

    public void Dispose()
    {
        _patternMatchVector.Dispose();
    }
}
