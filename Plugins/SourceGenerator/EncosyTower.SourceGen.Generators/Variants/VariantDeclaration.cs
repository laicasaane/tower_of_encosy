using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Variants
{
    public partial struct VariantDeclaration : IEquatable<VariantDeclaration>
    {
        /// <summary>Excluded from equality/hash — location is not stable across incremental runs.</summary>
        public Location location;

        /// <summary>
        /// Fully-qualified name of the wrapped type <c>T</c> (e.g. <c>global::System.Int32</c>).
        /// Used as the deduplication key across candidates.
        /// </summary>
        public string fullTypeName;

        /// <summary>
        /// Simple display name of the wrapped type (e.g. <c>Int32</c>), used for
        /// naming and code-generation purposes.
        /// </summary>
        public string typeName;

        /// <summary>
        /// Expression that retrieves the default converter instance for this variant
        /// (e.g. <c>MyInt32Variant.Converter.Default</c>).
        /// </summary>
        public string converterDefault;

        /// <summary>
        /// Byte size of the wrapped type when it is unmanaged; <see langword="null"/>
        /// if the type is managed or its size could not be determined.
        /// </summary>
        public int? unmanagedSize;

        /// <summary>
        /// Whether the wrapped type <c>T</c> is an unmanaged value type (struct).
        /// </summary>
        public bool isValueType;

        /// <summary>
        /// Whether the generated variant struct exposes an implicit conversion operator
        /// to the wrapped type <c>T</c>. <see langword="true"/> for value types and
        /// non-interface reference types.
        /// </summary>
        public bool hasImplicitFromStructToType;

        /// <summary>
        /// Short name of the <c>[Variant&lt;T&gt;]</c>-attributed struct
        /// (e.g. <c>Int32Variant</c>).
        /// </summary>
        public string structName;

        /// <summary>
        /// Fully-qualified name of the <c>[Variant&lt;T&gt;]</c>-attributed struct
        /// (e.g. <c>global::MyNamespace.Int32Variant</c>).
        /// </summary>
        public string structFullName;

        /// <summary>
        /// Unique hint name passed to <c>context.AddSource()</c> that identifies
        /// the generated source file for this variant.
        /// </summary>
        public string fileHintName;

        /// <summary>
        /// Namespace in which the <c>[Variant&lt;T&gt;]</c>-attributed struct is declared;
        /// empty string when in the global namespace.
        /// </summary>
        public string namespaceName;

        /// <summary>
        /// Ordered chain of containing type declarations (outer → inner) for nested
        /// struct declarations, each formatted as
        /// <c>&quot;&lt;accessibility&gt; partial &lt;keyword&gt; &lt;Name&gt;&quot;</c>.
        /// Empty when the struct is not nested.
        /// </summary>
        public EquatableArray<string> containingTypes;

        /// <summary>
        /// Whether all required fields were successfully populated during syntax and
        /// semantic analysis. Declarations where this is <see langword="false"/> are
        /// discarded before code generation.
        /// </summary>
        public bool isValid;

        public readonly bool IsValid
            => isValid
            && string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(typeName) == false
            && string.IsNullOrEmpty(structName) == false
            && string.IsNullOrEmpty(converterDefault) == false
            ;

        public readonly bool Equals(VariantDeclaration other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(converterDefault, other.converterDefault, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(structFullName, other.structFullName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && unmanagedSize == other.unmanagedSize
            && isValueType == other.isValueType
            && hasImplicitFromStructToType == other.hasImplicitFromStructToType
            && isValid == other.isValid
            && containingTypes.Equals(other.containingTypes)
            ;

        public readonly override bool Equals(object obj)
            => obj is VariantDeclaration other && Equals(other);

        public readonly override int GetHashCode()
        {
            return HashValue.Combine(
                  fullTypeName
                , typeName
                , converterDefault
                , structName
                , structFullName
                , fileHintName
                , namespaceName
                , unmanagedSize
            )
            .Add(isValueType)
            .Add(hasImplicitFromStructToType)
            ;
        }
    }
}
