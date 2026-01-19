using System;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public readonly struct OpType : IEquatable<OpType>
    {
        public readonly string Value;
        public readonly bool IsWrapper;

        public OpType(string value, bool isWrapper = false)
        {
            Value = value;
            IsWrapper = isWrapper;
        }

        public bool Equals(OpType other)
            => string.Equals(Value, other.Value) && IsWrapper == other.IsWrapper;

        public override bool Equals(object obj)
            => obj is OpType other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Value, IsWrapper);

        public override string ToString()
            => $"({Value}, IsWrapper={IsWrapper})";
    }
}
