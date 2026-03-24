using System;

namespace EncosyTower.SourceGen.Generators.Variants
{
    internal partial struct InternalVariantDeclaration : IEquatable<InternalVariantDeclaration>
    {
        /// <summary>Excluded from equality/hash — location is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>
        /// Fully-qualified name of the type <c>T</c> detected from usage of
        /// <c>Variant&lt;T&gt;.GetConverter()</c> or <c>CachedVariantConverter&lt;T&gt;.Default</c>
        /// (e.g. <c>global::System.Int32</c>). Used as the deduplication key across candidates.
        /// </summary>
        public string fullTypeName;

        /// <summary>
        /// Simple (unqualified) display name of the detected type (e.g. <c>Int32</c>),
        /// used in generated code.
        /// </summary>
        public string simpleTypeName;

        /// <summary>
        /// Name of the auto-generated internal variant struct
        /// (e.g. <c>Variant__Int32</c>).
        /// </summary>
        public string structName;

        /// <summary>
        /// Expression that retrieves the default converter instance for this internal
        /// variant (e.g. <c>Variant__Int32.Converter.Default</c>).
        /// </summary>
        public string converterDefault;

        /// <summary>
        /// Unique hint name passed to <c>context.AddSource()</c> that identifies
        /// the generated source file for this internal variant.
        /// </summary>
        public string fileHintName;

        /// <summary>
        /// Byte size of the detected type when it is unmanaged; <see langword="null"/>
        /// if the type is managed or its size could not be determined.
        /// </summary>
        public int? unmanagedSize;

        /// <summary>
        /// Whether the detected type <c>T</c> is an unmanaged value type (struct).
        /// </summary>
        public bool isValueType;

        /// <summary>
        /// Whether the generated internal variant struct exposes an implicit conversion
        /// operator to the original type <c>T</c>. <see langword="true"/> for value
        /// types and non-interface reference types.
        /// </summary>
        public bool hasImplicitFromStructToType;

        /// <summary>
        /// Whether all required fields were successfully populated during analysis.
        /// Declarations where this is <see langword="false"/> are discarded before
        /// code generation.
        /// </summary>
        public bool isValid;

        public readonly bool IsValid
            => isValid
            && string.IsNullOrEmpty(fullTypeName) == false
            && string.IsNullOrEmpty(simpleTypeName) == false
            && string.IsNullOrEmpty(structName) == false
            && string.IsNullOrEmpty(converterDefault) == false
            ;

        public readonly bool Equals(InternalVariantDeclaration other)
            => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
            && string.Equals(simpleTypeName, other.simpleTypeName, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(converterDefault, other.converterDefault, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && unmanagedSize == other.unmanagedSize
            && isValueType == other.isValueType
            && hasImplicitFromStructToType == other.hasImplicitFromStructToType
            && isValid == other.isValid
            ;

        public readonly override bool Equals(object obj)
            => obj is InternalVariantDeclaration other && Equals(other);

        public readonly override int GetHashCode()
        {
            return HashValue.Combine(
                  fullTypeName
                , simpleTypeName
                , structName
                , converterDefault
                , fileHintName
                , unmanagedSize
            )
            .Add(isValueType)
            .Add(hasImplicitFromStructToType)
            .Add(isValid)
            ;
        }
    }
}
