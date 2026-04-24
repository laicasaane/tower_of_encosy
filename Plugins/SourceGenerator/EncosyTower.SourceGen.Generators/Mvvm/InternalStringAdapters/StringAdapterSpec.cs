using System;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    public struct StringAdapterSpec : IEquatable<StringAdapterSpec>
    {
        public LocationInfo location;
        public string fullTypeName;
        public string simpleName;
        public string namespaceName;
        public string identifierName;
        public string labelName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(identifierName) == false;

        public readonly bool Equals(StringAdapterSpec other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(identifierName, other.identifierName, StringComparison.Ordinal)
            && string.Equals(labelName, other.labelName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is StringAdapterSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullTypeName, simpleName, namespaceName, identifierName, labelName);

        public static bool operator ==(StringAdapterSpec left, StringAdapterSpec right)
            => left.Equals(right);

        public static bool operator !=(StringAdapterSpec left, StringAdapterSpec right)
            => !left.Equals(right);
    }
}
