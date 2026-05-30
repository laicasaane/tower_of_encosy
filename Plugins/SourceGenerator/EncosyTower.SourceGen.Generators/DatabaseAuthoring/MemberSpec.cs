using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct MemberSpec : IEquatable<MemberSpec>
    {
        public string propertyName;
        public MemberPostConvertFlags postConvertFlags;
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
            && postConvertFlags.Equals(other.postConvertFlags)
            && type.Equals(other.type)
            && collection.Equals(other.collection)
            && converter.Equals(other.converter)
            && sheetConverter.Equals(other.sheetConverter)
            ;

        public readonly override bool Equals(object obj)
            => obj is MemberSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(propertyName, postConvertFlags, type, collection, converter, sheetConverter);
    }

    public struct MemberPostConvertFlags : IEquatable<MemberPostConvertFlags>
    {
        public bool defined;
        public bool emitRawStringProperty;

        public readonly bool DefineEmitRawStringProperty => defined && emitRawStringProperty;

        public readonly bool Equals(MemberPostConvertFlags other)
            => defined == other.defined && emitRawStringProperty == other.emitRawStringProperty;

        public readonly override bool Equals(object obj)
            => obj is MemberPostConvertFlags other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(defined, emitRawStringProperty);
    }
}
