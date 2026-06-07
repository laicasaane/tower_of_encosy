using System;

namespace EncosyTower.SourceGen.Generators.Entities.Lookups
{
    internal enum LookupKind : byte
    {
        None = 0,
        Buffer,
        Component,
        EnableableBuffer,
        EnableableComponent,
        PhysicsBuffer,
        PhysicsComponent,
        PhysicsEnableableComponent,
    }

    internal struct TypeRefSpec : IEquatable<TypeRefSpec>
    {
        public string typeName;
        public string typeIdentifier;
        public string typeShortName;
        public bool isReadOnly;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeIdentifier) == false
            ;

        public readonly override bool Equals(object obj)
            => obj is TypeRefSpec other && Equals(other);

        public readonly bool Equals(TypeRefSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeShortName, other.typeShortName, StringComparison.Ordinal)
            && isReadOnly == other.isReadOnly
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeIdentifier, typeShortName).Add(isReadOnly);
    }

    internal struct LookupSpec : IEquatable<LookupSpec>
    {
        public LocationInfo location;
        public string structName;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public string interfaceLookupRO;
        public string interfaceLookupRW;
        public LookupKind kind;
        public EquatableArray<TypeRefSpec> typeRefs;

        public readonly bool IsValid
            => kind != LookupKind.None
            && string.IsNullOrEmpty(structName) == false
            && typeRefs.Count > 0
            ;

        public readonly override bool Equals(object obj)
            => obj is LookupSpec other && Equals(other);

        public readonly bool Equals(LookupSpec other)
            => kind == other.kind
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(interfaceLookupRO, other.interfaceLookupRO, StringComparison.Ordinal)
            && string.Equals(interfaceLookupRW, other.interfaceLookupRW, StringComparison.Ordinal)
            && typeRefs.Equals(other.typeRefs)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  structName
                , interfaceLookupRO
                , interfaceLookupRW
                , kind
                , typeRefs
            );
    }
}
