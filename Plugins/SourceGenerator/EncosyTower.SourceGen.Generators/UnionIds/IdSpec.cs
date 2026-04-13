using System;
using EncosyTower.SourceGen.Common.UnionIds;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    public struct IdSpec : IEquatable<IdSpec>
    {
        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string fullName;
        public string simpleName;
        public string fileHintName;
        public string namespaceName;
        public EquatableArray<string> containingTypes;
        public Accessibility accessibility;
        public bool parentIsNamespace;
        public EquatableArray<KindSpec> inlineKinds;
        public UnionIdSize size;
        public string displayNameForId;
        public string displayNameForKind;
        public char separator;
        public UnionIdKindSettings kindSettings;
        public ParsableStructConverterSettings converterSettings;
        public int? fixedStringBytes;


        public readonly bool IsValid
            => string.IsNullOrEmpty(fullName) == false
            && string.IsNullOrEmpty(simpleName) == false;

        public readonly bool Equals(IdSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(displayNameForId, other.displayNameForId, StringComparison.Ordinal)
            && string.Equals(displayNameForKind, other.displayNameForKind, StringComparison.Ordinal)
            && containingTypes.Equals(other.containingTypes)
            && inlineKinds.Equals(other.inlineKinds)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && size == other.size
            && separator == other.separator
            && kindSettings == other.kindSettings
            && converterSettings == other.converterSettings
            && fixedStringBytes == other.fixedStringBytes
            ;

        public readonly override bool Equals(object obj)
            => obj is IdSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  fullName
                , simpleName
                , fileHintName
                , namespaceName
                , displayNameForId
                , displayNameForKind
            )
            .Add(containingTypes.GetHashCode())
            .Add(inlineKinds.GetHashCode())
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(size)
            .Add(separator)
            .Add(kindSettings)
            .Add(converterSettings)
            .Add(fixedStringBytes)
            ;
    }
}
