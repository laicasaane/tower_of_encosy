using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct HorizontalListSpec : IEquatable<HorizontalListSpec>
    {
        public string targetTypeFullName;
        public string containingTypeFullName;
        public EquatableArray<string> propertyNames;

        public readonly bool Equals(HorizontalListSpec other)
            => string.Equals(targetTypeFullName, other.targetTypeFullName, StringComparison.Ordinal)
            && string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
            && propertyNames.Equals(other.propertyNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is HorizontalListSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(targetTypeFullName, containingTypeFullName)
            .Add(propertyNames.GetHashCode());
    }
}
