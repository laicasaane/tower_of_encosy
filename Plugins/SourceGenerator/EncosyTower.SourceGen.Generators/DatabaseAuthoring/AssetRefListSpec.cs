using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct AssetRefListSpec : IEquatable<AssetRefListSpec>
    {
        public string tableTypeFullName;
        public string tableTypeSimpleName;
        public string dataTypeFullName;
        public string dataTypeSimpleName;
        public EquatableArray<string> fieldNames;

        public readonly bool Equals(AssetRefListSpec other)
            => string.Equals(tableTypeFullName, other.tableTypeFullName, StringComparison.Ordinal)
            && string.Equals(tableTypeSimpleName, other.tableTypeSimpleName, StringComparison.Ordinal)
            && string.Equals(dataTypeFullName, other.dataTypeFullName, StringComparison.Ordinal)
            && string.Equals(dataTypeSimpleName, other.dataTypeSimpleName, StringComparison.Ordinal)
            && fieldNames.Equals(other.fieldNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is AssetRefListSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(tableTypeFullName, tableTypeSimpleName, dataTypeFullName, dataTypeSimpleName)
            .Add(fieldNames.GetHashCode());
    }
}
