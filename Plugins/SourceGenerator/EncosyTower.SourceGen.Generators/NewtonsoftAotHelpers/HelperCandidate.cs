using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    /// <summary>
    /// Cache-friendly pipeline model for the NewtonsoftJsonAotHelper source generator.
    /// <para>
    /// Holds only primitive values and equatable collections — no
    /// <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> or
    /// <see cref="ISymbol"/> references — so that Roslyn's incremental generator
    /// engine can cache and compare instances cheaply across multiple compilations.
    /// </para>
    /// </summary>
    internal struct NewtonsoftAotHelperInfo : IEquatable<NewtonsoftAotHelperInfo>
    {
        /// <summary>
        /// Location of the declaration in source. Intentionally excluded from
        /// <see cref="Equals(NewtonsoftAotHelperInfo)"/> and <see cref="GetHashCode"/>:
        /// location data is not stable across incremental runs and must not drive
        /// cache invalidation.
        /// </summary>
        public LocationInfo location;

        /// <summary><c>symbol.Name</c> — the simple type name.</summary>
        public string typeName;

        /// <summary><c>symbol.ToFileName()</c> — used to build the hint name.</summary>
        public string fileHintName;

        /// <summary>
        /// <c>baseType.ToFullName()</c> — the fully-qualified name of the base type
        /// supplied to <c>[NewtonsoftJsonAotHelper(typeof(T))]</c>.
        /// </summary>
        public string baseTypeFullName;

        /// <summary>Containing namespace display string, or empty when in the global namespace.</summary>
        public string namespaceName;

        /// <summary>
        /// All types in the compilation that derive from <see cref="baseTypeFullName"/>,
        /// collected and extracted during the generator transform.
        /// </summary>
        public EquatableArray<AotTypeCandidate> typeCandidates;

        /// <summary>
        /// Ordered chain of containing type declaration headers (outer → inner),
        /// each formatted as <c>"&lt;accessibility&gt; partial &lt;keyword&gt; &lt;Name&gt;"</c>.
        /// Empty when the type is not nested inside another type.
        /// </summary>
        public EquatableArray<string> containingTypes;

        public bool isStatic;
        public bool isRecord;

        /// <summary>
        /// <see cref="TypeKind"/> of the annotated type (Class or Struct).
        /// Stored as a value-type enum; safe to cache.
        /// </summary>
        public TypeKind typeKind;

        public readonly bool IsValid => location.IsValid;

        public readonly bool Equals(NewtonsoftAotHelperInfo other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && string.Equals(baseTypeFullName, other.baseTypeFullName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && typeCandidates.Equals(other.typeCandidates)
            && containingTypes.Equals(other.containingTypes)
            && isStatic == other.isStatic
            && isRecord == other.isRecord
            && typeKind == other.typeKind
            ;

        public readonly override bool Equals(object obj)
            => obj is NewtonsoftAotHelperInfo other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, fileHintName, baseTypeFullName, namespaceName)
            .Add(typeCandidates.GetHashCode())
            .Add(containingTypes.GetHashCode())
            .Add(isStatic)
            .Add(isRecord)
            .Add(typeKind);
    }
}

