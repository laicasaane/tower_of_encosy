using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct MemberSpec : IEquatable<MemberSpec>
    {
        public string propertyName;
        public string typeFullName;
        public string typeSimpleName;

        public bool typeIsValueType;
        public bool typeHasParameterlessConstructor;
        public CollectionSpec collection;
        public ConverterSpec converter;

        public readonly CollectionSpec SelectCollection()
            => converter.kind == ConverterKind.None ? collection : converter.sourceCollection;

        public readonly string SelectTypeFullName()
            => converter.kind == ConverterKind.None ? typeFullName : converter.sourceTypeFullName;
        public readonly string SelectTypeSimpleName()
            => converter.kind == ConverterKind.None ? typeSimpleName : converter.sourceTypeSimpleName;

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
