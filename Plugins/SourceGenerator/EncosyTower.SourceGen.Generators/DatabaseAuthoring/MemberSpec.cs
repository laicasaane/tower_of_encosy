using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct MemberSpec : IEquatable<MemberSpec>
    {
        public string propertyName;
        public bool isPostConvert;
        public TypeSpec type;
        public CollectionSpec collection;
        public ConverterSpec converter;
        public ConverterSpec sheetConverter;

        public readonly TypeSpec SelectType()
            => converter.kind == ConverterKind.None ? type : converter.sourceType;

        public readonly CollectionSpec SelectCollection()
            => converter.kind == ConverterKind.None ? collection : converter.sourceCollection;

        public readonly bool Equals(MemberSpec other)
            => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && isPostConvert == other.isPostConvert
            && type.Equals(other.type)
            && collection.Equals(other.collection)
            && converter.Equals(other.converter)
            && sheetConverter.Equals(other.sheetConverter)
            ;

        public readonly override bool Equals(object obj)
            => obj is MemberSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(propertyName, isPostConvert, type, collection, converter, sheetConverter);
    }
}
