using System;

namespace EncosyTower.SourceGen.TypeModeling.Models
{
    public readonly struct AttributeModel : IEquatable<AttributeModel>
    {
        public readonly string FullName;
        public readonly string ShortName;
        public readonly EquatableArray<AttributeCtorArgModel> ConstructorArguments;
        public readonly EquatableArray<AttributeNamedArgModel> NamedArguments;

        public AttributeModel(
              string fullName
            , string shortName
            , EquatableArray<AttributeCtorArgModel> constructorArguments
            , EquatableArray<AttributeNamedArgModel> namedArguments
        )
        {
            FullName = fullName ?? string.Empty;
            ShortName = shortName ?? string.Empty;
            ConstructorArguments = constructorArguments;
            NamedArguments = namedArguments;
        }

        public bool Equals(AttributeModel other)
            => string.Equals(FullName, other.FullName, StringComparison.Ordinal)
            && string.Equals(ShortName, other.ShortName, StringComparison.Ordinal)
            && ConstructorArguments.Equals(other.ConstructorArguments)
            && NamedArguments.Equals(other.NamedArguments)
            ;

        public override bool Equals(object obj)
            => obj is AttributeModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(FullName, ShortName, ConstructorArguments, NamedArguments).ToHashCode();

        public static bool operator ==(AttributeModel left, AttributeModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeModel left, AttributeModel right)
            => left.Equals(right) == false;
    }
}
