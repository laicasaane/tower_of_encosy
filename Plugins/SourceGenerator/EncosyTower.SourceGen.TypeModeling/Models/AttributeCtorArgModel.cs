using System;
using Microsoft.CodeAnalysis;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct AttributeCtorArgModel : IEquatable<AttributeCtorArgModel>
    {
        public readonly TypedConstantKind Kind;
        public readonly string Value;
        public readonly EquatableArray<string> Elements;

        public AttributeCtorArgModel(
              TypedConstantKind kind
            , string value
            , EquatableArray<string> elements
        )
        {
            Kind = kind;
            Value = value ?? string.Empty;
            Elements = elements;
        }

        public bool Equals(AttributeCtorArgModel other)
            => Kind == other.Kind
            && string.Equals(Value, other.Value, StringComparison.Ordinal)
            && Elements.Equals(other.Elements)
            ;

        public override bool Equals(object obj)
            => obj is AttributeCtorArgModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(Kind, Value, Elements).ToHashCode();

        public static bool operator ==(AttributeCtorArgModel left, AttributeCtorArgModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeCtorArgModel left, AttributeCtorArgModel right)
            => left.Equals(right) == false;
    }
}
