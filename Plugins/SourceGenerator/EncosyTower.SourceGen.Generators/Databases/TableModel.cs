using System;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public readonly struct TableModel : IEquatable<TableModel>
    {
        public readonly string typeFullName;
        public readonly string typeName;
        public readonly string propertyName;
        public readonly NamingStrategy namingStrategy;

        public TableModel(
              string typeFullName
            , string typeName
            , string propertyName
            , NamingStrategy namingStrategy
        )
        {
            this.typeFullName = typeFullName;
            this.typeName = typeName;
            this.propertyName = propertyName;
            this.namingStrategy = namingStrategy;
        }

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeFullName) == false
            && string.IsNullOrEmpty(propertyName) == false;

        public readonly override bool Equals(object obj)
            => obj is TableModel other && Equals(other);

        public readonly bool Equals(TableModel other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && namingStrategy == other.namingStrategy;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, typeName, propertyName, (int)namingStrategy);
    }
}
