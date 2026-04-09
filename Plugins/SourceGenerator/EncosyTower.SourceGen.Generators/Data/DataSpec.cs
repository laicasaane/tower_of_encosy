using System;
using EncosyTower.SourceGen.Common.Data.Common;
using EncosyTower.SourceGen.TypeModeling.Models;
using Microsoft.CodeAnalysis;
using static EncosyTower.SourceGen.Common.Data.Common.Helpers;

namespace EncosyTower.SourceGen.Generators.Data
{
    public partial struct DataSpec : IEquatable<DataSpec>
    {
        /// <summary>Excluded from <see cref="Equals(DataSpec)"/> and <see cref="GetHashCode"/>
        /// — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string typeName;
        public string readOnlyTypeName;
        public string typeIdentifier;
        public string typeValidIdentifier;
        public string baseTypeName;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public TypeModel typeModel;
        public string idPropertyTypeName;
        public DataFieldPolicy fieldPolicy;
        public bool isMutable;
        public bool withoutPropertySetters;
        public bool withReadOnlyView;
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

        public readonly bool Equals(DataSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(readOnlyTypeName, other.readOnlyTypeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && string.Equals(typeValidIdentifier, other.typeValidIdentifier, StringComparison.Ordinal)
            && string.Equals(baseTypeName, other.baseTypeName, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(idPropertyTypeName, other.idPropertyTypeName, StringComparison.Ordinal)
            && typeModel.Equals(other.typeModel)
            && fieldPolicy == other.fieldPolicy
            && isMutable == other.isMutable
            && withoutPropertySetters == other.withoutPropertySetters
            && withReadOnlyView == other.withReadOnlyView
            && withoutId == other.withoutId
            && orders.Equals(other.orders)
            && fieldRefs.Equals(other.fieldRefs)
            && propRefs.Equals(other.propRefs)
            && overrideEquals.Equals(other.overrideEquals);

        public readonly override bool Equals(object obj)
            => obj is DataSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(typeName);
            hash.Add(readOnlyTypeName);
            hash.Add(typeIdentifier);
            hash.Add(typeValidIdentifier);
            hash.Add(baseTypeName);
            hash.Add(hintName);
            hash.Add(sourceFilePath);
            hash.Add(openingSource);
            hash.Add(closingSource);
            hash.Add(idPropertyTypeName);
            hash.Add(typeModel);
            hash.Add(fieldPolicy);
            hash.Add(isMutable);
            hash.Add(withoutPropertySetters);
            hash.Add(withReadOnlyView);
            hash.Add(withoutId);
            hash.Add(orders);
            hash.Add(fieldRefs);
            hash.Add(propRefs);
            hash.Add(overrideEquals);
            return hash.ToHashCode();
        }

        public readonly string AccessibilityKeyword => typeModel.Accessibility.ToKeyword();

        public readonly bool IsValueType => typeModel.TypeKind == TypeKind.Struct;

        public readonly bool IsSealed => typeModel.IsSealed || IsValueType;

        public readonly bool HasSerializableAttribute
        {
            get
            {
                var attrs = typeModel.Attributes;
                var count = attrs.Count;

                for (var i = 0; i < count; i++)
                {
                    if (string.Equals(
                          attrs[i].FullName
                        , SERIALIZABLE_ATTRIBUTE
                        , StringComparison.Ordinal
                    ))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public readonly bool HasGeneratePropertyBagAttribute
        {
            get
            {
                var attrs = typeModel.Attributes;
                var count = attrs.Count;

                for (var i = 0; i < count; i++)
                {
                    if (string.Equals(
                          attrs[i].FullName
                        , GENERATE_PROPERTY_BAG_ATTRIBUTE
                        , StringComparison.Ordinal
                    ))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public readonly bool HasGetHashCodeMethod
        {
            get
            {
                var methods = typeModel.Methods;
                var count = methods.Count;

                for (var i = 0; i < count; i++)
                {
                    var m = methods[i];

                    if (string.Equals(m.Name, "GetHashCode", StringComparison.Ordinal)
                        && m.Parameters.Count == 0
                        && m.ReturnsVoid == false
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public readonly bool HasEqualsMethod
        {
            get
            {
                var methods = typeModel.Methods;
                var count = methods.Count;

                for (var i = 0; i < count; i++)
                {
                    var m = methods[i];

                    if (string.Equals(m.Name, "Equals", StringComparison.Ordinal)
                        && m.Parameters.Count == 1
                        && string.Equals(m.ReturnTypeFullName, "bool", StringComparison.Ordinal)
                        && string.Equals(
                              m.Parameters[0].TypeFullName
                            , "object"
                            , StringComparison.Ordinal
                        )
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public readonly bool HasIEquatableMethod
        {
            get
            {
                var methods = typeModel.Methods;
                var count = methods.Count;

                for (var i = 0; i < count; i++)
                {
                    var m = methods[i];

                    if (string.Equals(m.Name, "Equals", StringComparison.Ordinal)
                        && m.Parameters.Count == 1
                        && string.Equals(m.ReturnTypeFullName, "bool", StringComparison.Ordinal)
                        && string.Equals(
                              m.Parameters[0].TypeFullName
                            , typeModel.FullName
                            , StringComparison.Ordinal
                        )
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
