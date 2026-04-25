using System;

namespace EncosyTower.SourceGen.Generators.Variants
{
    internal partial struct InternalVariantSpec : IEquatable<InternalVariantSpec>
    {
        public LocationInfo location;
        public string fullTypeName;
        public string simpleTypeName;
        public string structName;
        public string converterDefault;
        public string fileHintName;
        public int? unmanagedSize;
        public bool isValueType;
        public bool hasImplicitFromStructToType;
        public bool isValid;

        public readonly bool IsValid
            => isValid
            && string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(simpleTypeName) == false
            && string.IsNullOrEmpty(structName) == false
            && string.IsNullOrEmpty(converterDefault) == false
            ;

        public readonly bool Equals(InternalVariantSpec other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(simpleTypeName, other.simpleTypeName, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(converterDefault, other.converterDefault, StringComparison.Ordinal)
            && unmanagedSize == other.unmanagedSize
            && isValueType == other.isValueType
            && hasImplicitFromStructToType == other.hasImplicitFromStructToType
            && isValid == other.isValid
            ;

        public readonly override bool Equals(object obj)
            => obj is InternalVariantSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            return HashValue.Combine(
                  fullTypeName
                , simpleTypeName
                , structName
                , converterDefault
                , unmanagedSize
            )
            .Add(isValueType)
            .Add(hasImplicitFromStructToType)
            .Add(isValid)
            ;
        }
    }
}
