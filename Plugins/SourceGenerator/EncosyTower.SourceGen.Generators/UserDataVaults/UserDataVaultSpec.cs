using System;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal struct UserDataVaultSpec : IEquatable<UserDataVaultSpec>
    {
        public LocationInfo location;
        public string metadataName;
        public string className;
        public bool isStatic;
        public string namespaceName;
        public EquatableArray<string> containingTypeDeclarations;
        public string hintName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(metadataName) == false;

        public readonly bool Equals(UserDataVaultSpec other)
            => string.Equals(metadataName, other.metadataName, StringComparison.Ordinal)
            && isStatic == other.isStatic;

        public readonly override bool Equals(object obj)
            => obj is UserDataVaultSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(metadataName, isStatic);
    }
}
