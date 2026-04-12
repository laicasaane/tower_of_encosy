using System;
using EncosyTower.SourceGen.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    public struct KindSpec : IEquatable<KindSpec>
    {
        public LocationInfo location;


        public string kindFullName;
        public string kindSimpleName;
        public string idFullName;
        public string kindFullNameFromNullable;
        public bool isEnum;
        public bool kindEnumHasFlags;
        public string kindEnumUnderlyingTypeName;
        public bool kindEnumIsDisplayAttributeUsed;
        public EquatableArray<EnumMemberSpec> kindEnumValues;
        public int kindEnumFixedStringBytes;
        public bool hasExternalEnumExtensions;
        public string externalEnumExtensionsFullName;
        public bool isKindAlsoUnionId;
        public int kindUnmanagedSize;
        public MemberExistence tryParseSpan;
        public Equality equality;
        public bool hasToDisplayString;
        public bool hasToFixedString;
        public int toFixedStringBytes;
        public bool hasToDisplayFixedString;
        public int toDisplayFixedStringBytes;
        public string name;
        public string displayName;
        public ulong order;
        public ToStringMethods toStringMethods;
        public bool signed;

        public readonly bool Equals(KindSpec other)
            => string.Equals(kindFullName, other.kindFullName, StringComparison.Ordinal)
            && string.Equals(kindSimpleName, other.kindSimpleName, StringComparison.Ordinal)
            && string.Equals(idFullName, other.idFullName, StringComparison.Ordinal)
            && string.Equals(kindFullNameFromNullable, other.kindFullNameFromNullable, StringComparison.Ordinal)
            && string.Equals(kindEnumUnderlyingTypeName, other.kindEnumUnderlyingTypeName, StringComparison.Ordinal)
            && string.Equals(externalEnumExtensionsFullName, other.externalEnumExtensionsFullName, StringComparison.Ordinal)
            && string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && kindEnumValues.Equals(other.kindEnumValues)
            && isEnum == other.isEnum
            && kindEnumHasFlags == other.kindEnumHasFlags
            && kindEnumIsDisplayAttributeUsed == other.kindEnumIsDisplayAttributeUsed
            && kindEnumFixedStringBytes == other.kindEnumFixedStringBytes
            && hasExternalEnumExtensions == other.hasExternalEnumExtensions
            && isKindAlsoUnionId == other.isKindAlsoUnionId
            && kindUnmanagedSize == other.kindUnmanagedSize
            && tryParseSpan.Equals(other.tryParseSpan)
            && equality.Equals(other.equality)
            && hasToDisplayString == other.hasToDisplayString
            && hasToFixedString == other.hasToFixedString
            && toFixedStringBytes == other.toFixedStringBytes
            && hasToDisplayFixedString == other.hasToDisplayFixedString
            && toDisplayFixedStringBytes == other.toDisplayFixedStringBytes
            && order == other.order
            && toStringMethods == other.toStringMethods
            && signed == other.signed
            ;

        public readonly override bool Equals(object obj)
            => obj is KindSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  kindFullName
                , kindSimpleName
                , idFullName
                , kindFullNameFromNullable
                , name
                , displayName
            )
            .Add(kindEnumValues.GetHashCode())
            .Add(isEnum)
            .Add(kindEnumHasFlags)
            .Add(kindEnumFixedStringBytes)
            .Add(hasExternalEnumExtensions)
            .Add(isKindAlsoUnionId)
            .Add(kindUnmanagedSize)
            .Add(tryParseSpan.GetHashCode())
            .Add(equality.GetHashCode())
            .Add(hasToDisplayString)
            .Add(hasToFixedString)
            .Add(toFixedStringBytes)
            .Add(signed)
            ;
    }
}
