using System;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    /// <summary>
    /// Cache-friendly, equatable pipeline data extracted from a <c>[UserDataVault]</c>-attributed class.
    /// Replaces the former <c>UserDataVaultCandidate</c> struct as the incremental pipeline model.
    /// </summary>
    internal struct UserDataVaultInfo : IEquatable<UserDataVaultInfo>
    {
        /// <summary>Excluded from equality/hash — location is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>
        /// CLR metadata name (e.g. <c>"My.Namespace.MyVault"</c>) used as the equality key.
        /// </summary>
        public string metadataName;

        /// <summary>Simple class name (e.g. <c>"MyVault"</c>). Excluded from equality.</summary>
        public string className;

        /// <summary>Whether the vault class is declared <see langword="static"/>. Included in equality.</summary>
        public bool isStatic;

        /// <summary>
        /// Dotted namespace name for manual code reconstruction. Excluded from equality.
        /// </summary>
        public string namespaceName;

        /// <summary>
        /// Formatted containing-type declarations (e.g. <c>"public partial class Outer"</c>)
        /// ordered outermost-first, for manual opening-source reconstruction. Excluded from equality.
        /// </summary>
        public EquatableArray<string> containingTypeDeclarations;

        /// <summary>File-name-safe type name used to build the hint name. Excluded from equality.</summary>
        public string fileHintName;

        /// <summary>Pre-computed source hint name for <c>context.AddSource</c>. Excluded from equality.</summary>
        public string sourceHintName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(metadataName) == false;

        public readonly bool Equals(UserDataVaultInfo other)
            => string.Equals(metadataName, other.metadataName, StringComparison.Ordinal)
            && isStatic == other.isStatic;

        public readonly override bool Equals(object obj)
            => obj is UserDataVaultInfo other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(metadataName, isStatic);
    }
}
