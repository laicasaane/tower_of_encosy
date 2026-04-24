using System;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public struct TableSpec : IEquatable<TableSpec>
    {
        public string typeFullName;
        public string typeName;
        public string propertyName;
        public NamingStrategy namingStrategy;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeFullName) == false
            && string.IsNullOrEmpty(propertyName) == false;

        public readonly override bool Equals(object obj)
            => obj is TableSpec other && Equals(other);

        public readonly bool Equals(TableSpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && namingStrategy == other.namingStrategy;

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, typeName, propertyName, (int)namingStrategy);
    }
}
