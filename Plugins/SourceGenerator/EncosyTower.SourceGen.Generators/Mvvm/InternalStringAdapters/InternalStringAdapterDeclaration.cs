using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    public partial class InternalStringAdapterDeclaration
    {
        public ImmutableArray<StringAdapterCandidateInfo> Candidates { get; }

        public InternalStringAdapterDeclaration(
              ImmutableArray<StringAdapterCandidateInfo> candidates
            , ImmutableArray<string> existingAdapterTypeNames
        )
        {
            var typeFiltered = new Dictionary<string, StringAdapterCandidateInfo>();
            var typesToIgnore = new HashSet<string>(existingAdapterTypeNames, StringComparer.Ordinal);

            foreach (var candidate in candidates)
            {
                if (candidate.IsValid == false)
                {
                    continue;
                }

                var typeName = candidate.fullTypeName;

                if (typeName.ToUnionType().IsNativeUnionType()
                    || typesToIgnore.Contains(typeName)
                )
                {
                    continue;
                }

                if (typeFiltered.ContainsKey(typeName) == false)
                {
                    typeFiltered[typeName] = candidate;
                }
            }

            using var builder = ImmutableArrayBuilder<StringAdapterCandidateInfo>.Rent();
            builder.AddRange(typeFiltered.Values);
            Candidates = builder.ToImmutable();
        }
    }

    /// <summary>
    /// Cache-friendly, equatable pipeline model for a candidate type that needs
    /// an internal string adapter. Stores only primitive data extracted from the
    /// symbol — no <see cref="Microsoft.CodeAnalysis.ISymbol"/> or
    /// <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> references that would root
    /// the compilation graph and prevent GC.
    /// </summary>
    public struct StringAdapterCandidateInfo : IEquatable<StringAdapterCandidateInfo>
    {
        /// <summary>
        /// Excluded from <see cref="Equals(StringAdapterCandidateInfo)"/> and
        /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.
        /// </summary>
        public LocationInfo location;

        /// <summary>
        /// Fully-qualified name of the candidate type (e.g. <c>global::System.Int32</c>).
        /// Used as the deduplication key.
        /// </summary>
        public string fullTypeName;

        /// <summary>
        /// Simple (unqualified) display name of the candidate type (e.g. <c>Int32</c>).
        /// For non-generic types this equals <see cref="identifierName"/>.
        /// </summary>
        public string simpleName;

        /// <summary>
        /// The namespace of the candidate type (e.g. <c>System</c>).
        /// Used for the second argument of the <c>[Label(...)]</c> attribute.
        /// </summary>
        public string namespaceName;

        /// <summary>
        /// Valid C# identifier that encodes generic type arguments using look-alike bracket
        /// characters (e.g. <c>List&lt;int&gt;</c> → <c>Listᐸintᐳ</c>).
        /// Used as the base for the generated adapter class name so that different
        /// instantiations of the same open generic type produce distinct class names.
        /// </summary>
        public string identifierName;

        /// <summary>
        /// Human-readable display name of the type including generic notation
        /// (e.g. <c>List&lt;int&gt;</c>). Used in the first argument of the
        /// <c>[Label(...)]</c> attribute.
        /// </summary>
        public string labelName;

        public readonly bool IsValid
            => string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(identifierName) == false;

        public readonly bool Equals(StringAdapterCandidateInfo other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(identifierName, other.identifierName, StringComparison.Ordinal)
            && string.Equals(labelName, other.labelName, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is StringAdapterCandidateInfo other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullTypeName, simpleName, namespaceName, identifierName, labelName);

        public static bool operator ==(StringAdapterCandidateInfo left, StringAdapterCandidateInfo right)
            => left.Equals(right);

        public static bool operator !=(StringAdapterCandidateInfo left, StringAdapterCandidateInfo right)
            => !left.Equals(right);
    }
}
