using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public struct FieldDeclaration : IEquatable<FieldDeclaration>
    {
        public string name;
        public string typeName;
        public bool sameType;
        public bool isConst;
        public bool isStatic;
        public bool isReadOnly;

        public readonly bool IsValid
            => string.IsNullOrEmpty(name) == false
            && string.IsNullOrEmpty(typeName) == false;

        public static FieldDeclaration Create(IFieldSymbol field, INamedTypeSymbol fieldTypeSymbol)
        {
            return new FieldDeclaration {
                name = field.Name,
                typeName = field.Type.ToFullName(),
                sameType = SymbolEqualityComparer.Default.Equals(field.Type, fieldTypeSymbol),
                isConst = field.IsConst,
                isStatic = field.IsStatic,
                isReadOnly = field.IsReadOnly,
            };
        }

        public readonly bool Equals(FieldDeclaration other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal);

        public readonly override bool Equals(object obj)
            => obj is FieldDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, typeName);
    }
}
