using System;

namespace EncosyTower.SourceGen.Generators.Entities.TypeHandles
{
    internal enum TypeKind : byte
    {
        None = 0,
        Buffer,
        Component,
        SharedComponent,
    }

    internal struct TypeRefSpec : IEquatable<TypeRefSpec>
    {
        public string typeName;
        public string typeIdentifier;
        public string typeShortName;
        public bool isReadOnly;
        public TypeKind kind;

        public readonly bool IsValid
            => kind != TypeKind.None
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeIdentifier) == false
            ;

        public readonly override bool Equals(object obj)
            => obj is TypeRefSpec other && Equals(other);

        public readonly bool Equals(TypeRefSpec other)
            => kind == other.kind
            && isReadOnly == other.isReadOnly
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeShortName, other.typeShortName, StringComparison.Ordinal)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeIdentifier, typeShortName, isReadOnly, kind);
    }

    internal struct TypeHandleSpec : IEquatable<TypeHandleSpec>
    {
        public LocationInfo location;
        public string structName;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public EquatableArray<TypeRefSpec> typeRefs;

        public readonly bool IsValid
            => string.IsNullOrEmpty(structName) == false
            && typeRefs.Count > 0
            ;

        public readonly override bool Equals(object obj)
            => obj is TypeHandleSpec other && Equals(other);

        public readonly bool Equals(TypeHandleSpec other)
            => string.Equals(structName, other.structName, StringComparison.Ordinal)
            && typeRefs.Equals(other.typeRefs)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(structName, typeRefs);
    }
}
