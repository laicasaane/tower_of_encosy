using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct ConverterSpec : IEquatable<ConverterSpec>
    {
        public ConverterKind kind;
        public string converterTypeFullName;
        public CollectionSpec sourceCollection;
        public TypeSpec sourceType;
        public TypeSpec destType;

        public readonly string Convert(string expression)
        {
            if (string.IsNullOrEmpty(converterTypeFullName))
            {
                return expression;
            }

            if (kind == ConverterKind.Instance)
            {
                return $"new {converterTypeFullName}().Convert({expression})";
            }

            if (kind == ConverterKind.Static)
            {
                return $"{converterTypeFullName}.Convert({expression})";
            }

            return expression;
        }

        public readonly bool Equals(ConverterSpec other)
            => kind == other.kind
            && string.Equals(converterTypeFullName, other.converterTypeFullName, StringComparison.Ordinal)
            && sourceCollection.Equals(other.sourceCollection)
            && sourceType.Equals(other.sourceType)
            && destType.Equals(other.destType)
            ;

        public readonly override bool Equals(object obj)
            => obj is ConverterSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  kind
                , converterTypeFullName
                , sourceCollection
                , sourceType
                , destType
            );
    }
}
