using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct AttributeNamedArgModel : IEquatable<AttributeNamedArgModel>
    {
        public readonly string Name;
        public readonly TypedConstantKind Kind;
        public readonly string Value;

        public AttributeNamedArgModel(string name, TypedConstantKind kind, string value)
        {
            Name = name ?? string.Empty;
            Kind = kind;
            Value = value ?? string.Empty;
        }

        public bool Equals(AttributeNamedArgModel other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && Kind == other.Kind
            && string.Equals(Value, other.Value, StringComparison.Ordinal)
            ;

        public override bool Equals(object obj)
            => obj is AttributeNamedArgModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Name, Kind, Value).ToHashCode();

        public static bool operator ==(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeNamedArgModel left, AttributeNamedArgModel right)
            => left.Equals(right) == false;
    }
}
