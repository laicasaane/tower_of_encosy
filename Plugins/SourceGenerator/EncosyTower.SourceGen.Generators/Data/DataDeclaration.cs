using System;
using EncosyTower.SourceGen.Common.Data.Common;

namespace EncosyTower.SourceGen.Generators.Data
{
    public partial struct DataDeclaration : IEquatable<DataDeclaration>
    {
        /// <summary>Excluded from <see cref="Equals(DataDeclaration)"/> and <see cref="GetHashCode"/>
        /// — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string typeName;
        public string readOnlyTypeName;
        public string typeIdentifier;
        public string typeValidIdentifier;
        public string baseTypeName;
        public string accessibilityKeyword;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public string idPropertyTypeName;
        public DataFieldPolicy fieldPolicy;
        public bool isMutable;
        public bool isValueType;
        public bool withoutPropertySetters;
        public bool withReadOnlyView;
        public bool isSealed;
        public bool hasSerializableAttribute;
        public bool hasGeneratePropertyBagAttribute;
        public bool hasGetHashCodeMethod;
        public bool hasEqualsMethod;
        public bool hasIEquatableMethod;
        public bool withoutId;
        public EquatableArray<OrderData> orders;
        public EquatableArray<FieldRefData> fieldRefs;
        public EquatableArray<PropRefData> propRefs;
        public EquatableArray<string> overrideEquals;

        public readonly bool HasBaseType
            => string.IsNullOrEmpty(baseTypeName) == false;

        public readonly bool HasIdProperty
            => string.IsNullOrEmpty(idPropertyTypeName) == false;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(hintName) == false;

        public readonly bool Equals(DataDeclaration other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(readOnlyTypeName, other.readOnlyTypeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeValidIdentifier, other.typeValidIdentifier, StringComparison.Ordinal)
            && string.Equals(baseTypeName, other.baseTypeName, StringComparison.Ordinal)
            && string.Equals(accessibilityKeyword, other.accessibilityKeyword, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(idPropertyTypeName, other.idPropertyTypeName, StringComparison.Ordinal)
            && fieldPolicy == other.fieldPolicy
            && isMutable == other.isMutable
            && isValueType == other.isValueType
            && withoutPropertySetters == other.withoutPropertySetters
            && withReadOnlyView == other.withReadOnlyView
            && isSealed == other.isSealed
            && hasSerializableAttribute == other.hasSerializableAttribute
            && hasGeneratePropertyBagAttribute == other.hasGeneratePropertyBagAttribute
            && hasGetHashCodeMethod == other.hasGetHashCodeMethod
            && hasEqualsMethod == other.hasEqualsMethod
            && hasIEquatableMethod == other.hasIEquatableMethod
            && withoutId == other.withoutId
            && orders.Equals(other.orders)
            && fieldRefs.Equals(other.fieldRefs)
            && propRefs.Equals(other.propRefs)
            && overrideEquals.Equals(other.overrideEquals);

        public readonly override bool Equals(object obj)
            => obj is DataDeclaration other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(typeName);
            hash.Add(readOnlyTypeName);
            hash.Add(typeIdentifier);
            hash.Add(typeValidIdentifier);
            hash.Add(baseTypeName);
            hash.Add(accessibilityKeyword);
            hash.Add(hintName);
            hash.Add(sourceFilePath);
            hash.Add(openingSource);
            hash.Add(closingSource);
            hash.Add(idPropertyTypeName);
            hash.Add(fieldPolicy);
            hash.Add(isMutable);
            hash.Add(isValueType);
            hash.Add(withoutPropertySetters);
            hash.Add(withReadOnlyView);
            hash.Add(isSealed);
            hash.Add(hasSerializableAttribute);
            hash.Add(hasGeneratePropertyBagAttribute);
            hash.Add(hasGetHashCodeMethod);
            hash.Add(hasEqualsMethod);
            hash.Add(hasIEquatableMethod);
            hash.Add(withoutId);
            hash.Add(orders);
            hash.Add(fieldRefs);
            hash.Add(propRefs);
            hash.Add(overrideEquals);
            return hash.ToHashCode();
        }
    }
}
