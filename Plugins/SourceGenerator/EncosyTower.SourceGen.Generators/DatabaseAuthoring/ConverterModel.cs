using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct ConverterModel : IEquatable<ConverterModel>
    {
        public ConverterKind kind;
        public string converterTypeFullName;

        // The source type (what the converter reads FROM)
        public CollectionModel sourceCollection;
        public string sourceTypeFullName;
        public string sourceTypeSimpleName;
        public bool sourceTypeIsValueType;
        public bool sourceTypeHasParameterlessConstructor;

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

        public readonly bool Equals(ConverterModel other)
            => kind == other.kind
            && string.Equals(converterTypeFullName, other.converterTypeFullName, StringComparison.Ordinal)
            && sourceCollection.Equals(other.sourceCollection)
            && string.Equals(sourceTypeFullName, other.sourceTypeFullName, StringComparison.Ordinal)
            && string.Equals(sourceTypeSimpleName, other.sourceTypeSimpleName, StringComparison.Ordinal)
            && sourceTypeIsValueType == other.sourceTypeIsValueType
            && sourceTypeHasParameterlessConstructor == other.sourceTypeHasParameterlessConstructor
            ;

        public readonly override bool Equals(object obj)
            => obj is ConverterModel other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(converterTypeFullName, sourceTypeFullName, sourceTypeSimpleName)
            .Add((byte)kind)
            .Add(sourceCollection.GetHashCode())
            .Add(sourceTypeIsValueType)
            .Add(sourceTypeHasParameterlessConstructor);
    }
}
