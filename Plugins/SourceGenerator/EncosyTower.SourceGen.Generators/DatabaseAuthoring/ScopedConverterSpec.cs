using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct ScopedConverterSpec : IEquatable<ScopedConverterSpec>
    {
        public string declaringDataTypeFullName;
        public string propertyName;
        public ConverterSpec converter;
        public ConverterSpec sheetConverter;
        public HashValue64 scopeKey;

        public readonly bool IsValid
            => string.IsNullOrEmpty(declaringDataTypeFullName) == false
            && string.IsNullOrEmpty(propertyName) == false;

        public static string MakeMemberKey(string declaringDataTypeFullName, string propertyName)
            => $"{declaringDataTypeFullName}-{propertyName}";

        public readonly bool Equals(ScopedConverterSpec other)
            => scopeKey == other.scopeKey
            && string.Equals(declaringDataTypeFullName, other.declaringDataTypeFullName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && converter.Equals(other.converter)
            && sheetConverter.Equals(other.sheetConverter)
            ;

        public readonly override bool Equals(object obj)
            => obj is ScopedConverterSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  scopeKey
                , declaringDataTypeFullName
                , propertyName
                , converter
                , sheetConverter
            );
    }
}
