using System;

namespace EncosyTower.SourceGen.Helpers.Data
{
    public struct ForwardedAttributeData : IEquatable<ForwardedAttributeData>
    {
        public string fullTypeName;
        public string syntax;

        public readonly bool Equals(ForwardedAttributeData other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(syntax, other.syntax, StringComparison.Ordinal);

        public readonly override bool Equals(object obj)
            => obj is ForwardedAttributeData other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullTypeName, syntax);
    }
}
