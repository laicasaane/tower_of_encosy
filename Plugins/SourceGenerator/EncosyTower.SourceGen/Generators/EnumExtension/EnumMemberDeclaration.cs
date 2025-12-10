using System;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    public struct EnumMemberDeclaration : IEquatable<EnumMemberDeclaration>
    {
        public string name;
        public string displayName;
        public ulong order;

        public readonly bool IsValid
            => string.IsNullOrEmpty(name) == false
            && string.IsNullOrEmpty(displayName) == false;

        public readonly bool Equals(EnumMemberDeclaration other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && order == other.order;

        public readonly override bool Equals(object obj)
            => obj is EnumMemberDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, displayName, order);

        public readonly override string ToString()
            => name;
    }
}
