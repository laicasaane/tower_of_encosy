using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    /// <summary>
    /// One horizontal-list mapping entry (<c>[Horizontal]</c> attribute data).
    /// Maps a target data type + containing table type to a set of array property names
    /// that should be treated as horizontal lists (i.e. <c>List&lt;T&gt;</c> instead of
    /// <c>VerticalList&lt;T&gt;</c>).
    /// </summary>
    public struct HorizontalListEntry : IEquatable<HorizontalListEntry>
    {
        /// <summary>Full name of the data type that has the horizontal-list property.</summary>
        public string targetTypeFullName;

        /// <summary>Full name of the DataTableAsset type whose rows carry the horizontal list.</summary>
        public string containingTypeFullName;

        /// <summary>Property names that should use a <c>List&lt;T&gt;</c> instead of <c>VerticalList&lt;T&gt;</c>.</summary>
        public EquatableArray<string> propertyNames;

        public readonly bool Equals(HorizontalListEntry other)
            => string.Equals(targetTypeFullName, other.targetTypeFullName, StringComparison.Ordinal)
            && string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
            && propertyNames.Equals(other.propertyNames)
            ;

        public readonly override bool Equals(object obj)
            => obj is HorizontalListEntry other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(targetTypeFullName, containingTypeFullName)
            .Add(propertyNames.GetHashCode());
    }
}
