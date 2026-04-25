using System;

namespace EncosyTower.SourceGen.Generators.Variants
{
    public partial struct VariantSpec : IEquatable<VariantSpec>
    {
        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string fullTypeName;
        public string typeName;
        public string converterDefault;
        public int? unmanagedSize;
        public bool isValueType;
        public bool hasImplicitFromStructToType;
        public string structName;
        public string structFullName;
        public string fileHintName;
        public string namespaceName;
        public EquatableArray<string> containingTypes;
        public bool isValid;

        public readonly bool IsValid
            => isValid
            && string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(structName) == false
            && string.IsNullOrEmpty(converterDefault) == false
            ;

        public readonly bool Equals(VariantSpec other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(converterDefault, other.converterDefault, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(structFullName, other.structFullName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && unmanagedSize == other.unmanagedSize
            && isValueType == other.isValueType
            && hasImplicitFromStructToType == other.hasImplicitFromStructToType
            && isValid == other.isValid
            && containingTypes.Equals(other.containingTypes)
            ;

        public readonly override bool Equals(object obj)
            => obj is VariantSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            return HashValue.Combine(
                  fullTypeName
                , typeName
                , converterDefault
                , structName
                , structFullName
                , namespaceName
                , unmanagedSize
            )
            .Add(isValueType)
            .Add(hasImplicitFromStructToType)
            .Add(isValid)
            .Add(containingTypes.GetHashCode())
            ;
        }
    }
}
