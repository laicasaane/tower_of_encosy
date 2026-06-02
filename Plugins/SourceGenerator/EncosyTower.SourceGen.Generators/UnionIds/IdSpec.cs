using System;
using EncosyTower.SourceGen.Helpers.UnionIds;
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
        public string displayNameForId;
        public string displayNameForKind;
        public EquatableArray<string> containingTypes;
        public EquatableArray<KindSpec> inlineKinds;
        public Accessibility accessibility;
        public int? fixedStringBytes;
        public char separator;
        public bool parentIsNamespace;
        public bool generateTryFormat;
        public UnionIdSize size;
        public UnionIdKindSettings kindSettings;
        public ParsableStructConverterSettings converterSettings;


        public readonly bool IsValid
            => string.IsNullOrEmpty(fullName) == false
            && string.IsNullOrEmpty(simpleName) == false;

        public readonly bool Equals(IdSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(displayNameForId, other.displayNameForId, StringComparison.Ordinal)
            && string.Equals(displayNameForKind, other.displayNameForKind, StringComparison.Ordinal)
            && containingTypes.Equals(other.containingTypes)
            && inlineKinds.Equals(other.inlineKinds)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && generateTryFormat == other.generateTryFormat
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
                , namespaceName
                , displayNameForId
                , displayNameForKind
                , containingTypes
                , inlineKinds
            )
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(generateTryFormat)
            .Add(size)
            .Add(separator)
            .Add(kindSettings)
            .Add(converterSettings)
            .Add(fixedStringBytes)
            ;
    }
}
