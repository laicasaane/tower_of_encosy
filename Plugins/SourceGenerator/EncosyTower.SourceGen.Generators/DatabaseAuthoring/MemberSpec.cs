using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct MemberSpec : IEquatable<MemberSpec>
    {
        public string propertyName;
        public MemberManualAuthoring manualAuthoring;
        public TypeSpec type;
        public CollectionSpec collection;
        public ConverterSpec memberConverter;
        public ConverterSpec localConverter;
        public ConverterSpec converter;
        public ConverterSpec sheetConverter;

        public readonly TypeSpec SelectType()
            => converter.kind == ConverterKind.None ? type : converter.sourceType;

        public readonly CollectionSpec SelectCollection()
            => converter.kind == ConverterKind.None ? collection : converter.sourceCollection;

        public readonly bool Equals(MemberSpec other)
            => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
            && manualAuthoring.Equals(other.manualAuthoring)
            && type.Equals(other.type)
            && collection.Equals(other.collection)
            && memberConverter.Equals(other.memberConverter)
            && localConverter.Equals(other.localConverter)
            && converter.Equals(other.converter)
            && sheetConverter.Equals(other.sheetConverter)
            ;

        public readonly override bool Equals(object obj)
            => obj is MemberSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  propertyName
                , manualAuthoring
                , type
                , collection
                , memberConverter
                , localConverter
                , converter
                , sheetConverter
            );
    }

    public struct MemberManualAuthoring : IEquatable<MemberManualAuthoring>
    {
        public const string SUFFIX = "_Manual";

        public bool defined;
        public TypeSpec type;
        public CollectionSpec collection;

        public readonly bool Equals(MemberManualAuthoring other)
            => defined == other.defined
            && type.Equals(other.type)
            && collection.Equals(other.collection)
            ;

        public readonly override bool Equals(object obj)
            => obj is MemberManualAuthoring other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(defined, type, collection);
    }
}
