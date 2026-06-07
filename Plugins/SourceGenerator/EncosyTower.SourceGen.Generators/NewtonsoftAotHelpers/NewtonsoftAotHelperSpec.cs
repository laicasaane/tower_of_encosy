using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers
{
    internal struct NewtonsoftAotHelperSpec : IEquatable<NewtonsoftAotHelperSpec>
    {
        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string typeName;
        public string hintName;
        public string baseTypeFullName;
        public string namespaceName;
        public EquatableArray<AotTypeSpec> typeCandidates;
        public EquatableArray<string> containingTypes;
        public bool isStatic;
        public bool isRecord;
        public TypeKind typeKind;

        public readonly bool IsValid => location.IsValid;

        public readonly bool Equals(NewtonsoftAotHelperSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(baseTypeFullName, other.baseTypeFullName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && typeCandidates.Equals(other.typeCandidates)
            && containingTypes.Equals(other.containingTypes)
            && isStatic == other.isStatic
            && isRecord == other.isRecord
            && typeKind == other.typeKind
            ;

        public readonly override bool Equals(object obj)
            => obj is NewtonsoftAotHelperSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, hintName, baseTypeFullName, namespaceName)
            .Add(typeCandidates.GetHashCode())
            .Add(containingTypes.GetHashCode())
            .Add(isStatic)
            .Add(isRecord)
            .Add(typeKind);
    }
}

