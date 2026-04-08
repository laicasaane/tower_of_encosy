using System;
using EncosyTower.SourceGen.TypeModeling.Internal;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct AttributeModel : IEquatable<AttributeModel>
    {
        public readonly string FullName;
        public readonly EquatableArray<string> ConstructorArguments;
        public readonly EquatableArray<AttributeNamedArgModel> NamedArguments;

        public AttributeModel(
              string fullName
            , EquatableArray<string> constructorArguments
            , EquatableArray<AttributeNamedArgModel> namedArguments
        )
        {
            FullName = fullName ?? string.Empty;
            ConstructorArguments = constructorArguments;
            NamedArguments = namedArguments;
        }

        public bool Equals(AttributeModel other)
            => string.Equals(FullName, other.FullName, StringComparison.Ordinal)
            && ConstructorArguments.Equals(other.ConstructorArguments)
            && NamedArguments.Equals(other.NamedArguments)
            ;

        public override bool Equals(object obj)
            => obj is AttributeModel other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(FullName, ConstructorArguments, NamedArguments).ToHashCode();

        public static bool operator ==(AttributeModel left, AttributeModel right)
            => left.Equals(right);

        public static bool operator !=(AttributeModel left, AttributeModel right)
            => left.Equals(right) == false;
    }
}
