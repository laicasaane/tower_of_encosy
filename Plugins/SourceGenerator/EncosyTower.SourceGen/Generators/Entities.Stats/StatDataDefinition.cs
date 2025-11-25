using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatDataDefinition : IEquatable<StatDataDefinition>
    {
        public StructDeclarationSyntax syntax;
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public string valueTypeName;
        public string valueFullTypeName;
        public string underlyingTypeName;
        public bool singleValue;
        public bool isEnum;

        public readonly bool IsValid
            => syntax != null
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && string.IsNullOrEmpty(valueTypeName) == false
            && string.IsNullOrEmpty(valueFullTypeName) == false
            ;

        public readonly override bool Equals(object obj)
            => obj is StatDataDefinition other && Equals(other);

        public readonly bool Equals(StatDataDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && string.Equals(valueTypeName, other.valueTypeName, StringComparison.Ordinal)
            && string.Equals(valueFullTypeName, other.valueFullTypeName, StringComparison.Ordinal)
            && string.Equals(underlyingTypeName, other.underlyingTypeName, StringComparison.Ordinal)
            && singleValue == other.singleValue
            && isEnum == other.isEnum
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , valueTypeName
                , valueFullTypeName
                , underlyingTypeName
                , singleValue
                , isEnum
            );
    }
}
