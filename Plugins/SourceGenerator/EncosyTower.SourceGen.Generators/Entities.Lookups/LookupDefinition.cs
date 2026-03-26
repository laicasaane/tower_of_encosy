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

    internal struct TypeRefModel : IEquatable<TypeRefModel>
    {
        /// <summary>Full qualified type name (e.g. global::My.Namespace.MyType)</summary>
        public string typeName;

        /// <summary>Valid C# identifier used to build field names (e.g. My_Namespace_MyType)</summary>
        public string typeIdentifier;

        /// <summary>Simple type name used for region labels (e.g. MyType)</summary>
        public string typeShortName;

        public bool isReadOnly;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(typeIdentifier) == false
            ;

        public readonly override bool Equals(object obj)
            => obj is TypeRefModel other && Equals(other);

        public readonly bool Equals(TypeRefModel other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeShortName, other.typeShortName, StringComparison.Ordinal)
            && isReadOnly == other.isReadOnly
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeIdentifier, typeShortName).Add(isReadOnly);
    }

    internal struct LookupDefinition : IEquatable<LookupDefinition>
    {
        /// <summary>Excluded from <see cref="Equals(LookupDefinition)"/> and
        /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string structName;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;

        public string interfaceLookupRO;
        public string interfaceLookupRW;
        public LookupKind kind;
        public EquatableArray<TypeRefModel> typeRefs;

        public readonly bool IsValid
            => kind != LookupKind.None
            && string.IsNullOrEmpty(structName) == false
            && string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(sourceFilePath) == false
            && string.IsNullOrEmpty(openingSource) == false
            && string.IsNullOrEmpty(closingSource) == false
            && typeRefs.Count > 0
            ;

        public readonly override bool Equals(object obj)
            => obj is LookupDefinition other && Equals(other);

        public readonly bool Equals(LookupDefinition other)
            => kind == other.kind
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(interfaceLookupRO, other.interfaceLookupRO, StringComparison.Ordinal)
            && string.Equals(interfaceLookupRW, other.interfaceLookupRW, StringComparison.Ordinal)
            && typeRefs.Equals(other.typeRefs)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  structName
                , hintName
                , sourceFilePath
                , openingSource
                , closingSource
                , interfaceLookupRO
                , interfaceLookupRW
            )
            .Add((byte)kind)
            .Add(typeRefs.GetHashCode())
            ;
    }
}
