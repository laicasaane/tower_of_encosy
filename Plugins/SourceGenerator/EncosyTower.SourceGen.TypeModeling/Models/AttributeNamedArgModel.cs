using System;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct AttributeNamedArgModel : IEquatable<AttributeNamedArgModel>
    {
        public readonly string Name;
        public readonly string Value;

        public AttributeNamedArgModel(string name, string value)
        {
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public bool Equals(AttributeNamedArgModel other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(Value, other.Value, StringComparison.Ordinal)
            ;

        public override bool Equals(object obj)
            => obj is AttributeNamedArgModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Name, Value).ToHashCode();

        public static bool operator ==(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => left.Equals(right) == false;
    }
}
