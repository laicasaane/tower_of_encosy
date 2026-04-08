using System;

namespace EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers
{
    internal enum AotCollectionKind
    {
        None,
        List,
        HashSet,
        Dictionary,
    }

    /// <summary>
    /// Cache-friendly representation of a single type argument used in AOT
    /// helper code generation.
    /// </summary>
    internal struct AotTypeArgSpec : IEquatable<AotTypeArgSpec>
    {
        /// <summary>Fully-qualified name (e.g. <c>global::Foo.Bar</c>).</summary>
        public string fullName;

        /// <summary>
        /// Pre-evaluated: <c>true</c> when <c>AotHelper.EnsureType&lt;T&gt;()</c>
        /// should be emitted for this type.
        /// </summary>
        public bool canEnsure;

        public readonly bool Equals(AotTypeArgSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && canEnsure == other.canEnsure;

        public readonly override bool Equals(object obj)
            => obj is AotTypeArgSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName).Add(canEnsure);
    }

    /// <summary>
    /// Cache-friendly representation of a single field extracted from a type's
    /// full inheritance hierarchy, capturing all data needed by
    /// <see cref="HelperDeclaration"/> to emit the AOT registration calls.
    /// </summary>
    internal struct AotFieldSpec : IEquatable<AotFieldSpec>
    {
        /// <summary>Fully-qualified name of the field's declared type.</summary>
        public string fieldTypeFullName;

        /// <summary>
        /// Pre-evaluated: <c>true</c> when <c>AotHelper.EnsureType&lt;fieldType&gt;()</c>
        /// should be emitted.
        /// </summary>
        public bool fieldTypeCanEnsure;

        /// <summary>
        /// Collection classification used to choose the correct
        /// <c>AotHelper.EnsureList / EnsureDictionary</c> overload.
        /// <see cref="AotCollectionKind.None"/> for non-collection or unknown generics.
        /// </summary>
        public AotCollectionKind collectionKind;

        /// <summary>
        /// For <see cref="AotCollectionKind.List"/> and
        /// <see cref="AotCollectionKind.HashSet"/>: the element type arg.
        /// For <see cref="AotCollectionKind.Dictionary"/>: the key type arg.
        /// Unused for <see cref="AotCollectionKind.None"/>.
        /// </summary>
        public AotTypeArgSpec elementOrKey;

        /// <summary>
        /// The value type argument for <see cref="AotCollectionKind.Dictionary"/>.
        /// Unused for all other collection kinds.
        /// </summary>
        public AotTypeArgSpec dictionaryValue;

        /// <summary>
        /// Named type arguments for a generic type that is neither a
        /// Dictionary nor a List/HashSet.  Empty when
        /// <see cref="collectionKind"/> is not <see cref="AotCollectionKind.None"/>
        /// or when the field type is non-generic / unbound-generic.
        /// </summary>
        public EquatableArray<AotTypeArgSpec> otherTypeArgs;

        public readonly bool Equals(AotFieldSpec other)
            => string.Equals(fieldTypeFullName, other.fieldTypeFullName, StringComparison.Ordinal)
            && fieldTypeCanEnsure == other.fieldTypeCanEnsure
            && collectionKind == other.collectionKind
            && elementOrKey.Equals(other.elementOrKey)
            && dictionaryValue.Equals(other.dictionaryValue)
            && otherTypeArgs.Equals(other.otherTypeArgs)
            ;

        public readonly override bool Equals(object obj)
            => obj is AotFieldSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fieldTypeFullName)
            .Add(fieldTypeCanEnsure)
            .Add(collectionKind)
            .Add(elementOrKey.GetHashCode())
            .Add(dictionaryValue.GetHashCode())
            .Add(otherTypeArgs.GetHashCode());
    }

    /// <summary>
    /// Cache-friendly pipeline model for a single type candidate in the
    /// NewtonsoftJsonAotHelper source generator.
    /// <para>
    /// Holds only primitive values and equatable collections — no
    /// <see cref="Microsoft.CodeAnalysis.SyntaxNode"/> or
    /// <see cref="Microsoft.CodeAnalysis.ISymbol"/> references — so that Roslyn's
    /// incremental generator engine can cache and compare instances cheaply
    /// across multiple compilations.
    /// </para>
    /// </summary>
    internal struct AotTypeSpec : IEquatable<AotTypeSpec>
    {
        /// <summary>Fully-qualified name of the type (e.g. <c>global::Foo.MyType</c>).</summary>
        public string fullName;

        /// <summary>
        /// All declared fields across the full type hierarchy (type + every
        /// non-system base type), pre-flattened for code generation.
        /// </summary>
        public EquatableArray<AotFieldSpec> fields;

        public readonly bool IsValid => fullName is not null;

        public readonly bool Equals(AotTypeSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && fields.Equals(other.fields)
            ;

        public readonly override bool Equals(object obj)
            => obj is AotTypeSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName)
            .Add(fields.GetHashCode());
    }
}
