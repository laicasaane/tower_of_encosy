using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    internal partial struct StatSystemDefinition : IEquatable<StatSystemDefinition>
    {
        public string typeName;
        public string typeNamespace;
        public string typeIdentifier;
        public TypeDeclarationSyntax syntax;
        public int maxDataSize;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeNamespace) == false
            && syntax != null
            ;

        public readonly override bool Equals(object obj)
            => obj is StatSystemDefinition other && Equals(other);

        public readonly bool Equals(StatSystemDefinition other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeNamespace, other.typeNamespace, StringComparison.Ordinal)
            && maxDataSize == other.maxDataSize
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , typeNamespace
                , maxDataSize
            );
    }
}
