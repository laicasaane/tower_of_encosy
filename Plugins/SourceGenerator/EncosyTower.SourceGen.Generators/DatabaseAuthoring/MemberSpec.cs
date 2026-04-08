using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct MemberSpec : IEquatable<MemberSpec>
    {
        public string propertyName;

        /// <summary>Full name of the property/field type (original, not converter source).</summary>
        public string typeFullName;

        /// <summary>Simple name of the property/field type.</summary>
        public string typeSimpleName;

        public bool typeIsValueType;

        /// <summary>
        /// Whether the original type has a public parameterless constructor.
        /// Always reflects the base type, even when a converter is in use.
        /// </summary>
        public bool typeHasParameterlessConstructor;

        /// <summary>Collection info for the original (non-converted) type.</summary>
        public CollectionSpec collection;

        /// <summary>Converter info. <see cref="ConverterSpec.kind"/> == None when no converter.</summary>
        public ConverterSpec converter;

        /// <summary>Returns the effective collection (converter source if converter is active, otherwise original).</summary>
        public readonly CollectionSpec SelectCollection()
            => converter.kind == ConverterKind.None ? collection : converter.sourceCollection;

        /// <summary>Returns the effective type full name.</summary>
        public readonly string SelectTypeFullName()
            => converter.kind == ConverterKind.None ? typeFullName : converter.sourceTypeFullName;

        /// <summary>Returns the effective type simple name.</summary>
        public readonly string SelectTypeSimpleName()
            => converter.kind == ConverterKind.None ? typeSimpleName : converter.sourceTypeSimpleName;

        /// <summary>Returns whether the effective type is a value type.</summary>
        public readonly bool SelectTypeIsValueType()
            => converter.kind == ConverterKind.None ? typeIsValueType : converter.sourceTypeIsValueType;

        public readonly bool Equals(MemberSpec other)
            => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeSimpleName, other.typeSimpleName, StringComparison.Ordinal)
            && typeIsValueType == other.typeIsValueType
            && typeHasParameterlessConstructor == other.typeHasParameterlessConstructor
            && collection.Equals(other.collection)
            && converter.Equals(other.converter)
            ;

        public readonly override bool Equals(object obj)
            => obj is MemberSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(propertyName, typeFullName, typeSimpleName)
            .Add(typeIsValueType)
            .Add(typeHasParameterlessConstructor)
            .Add(collection.GetHashCode())
            .Add(converter.GetHashCode());
    }
}
