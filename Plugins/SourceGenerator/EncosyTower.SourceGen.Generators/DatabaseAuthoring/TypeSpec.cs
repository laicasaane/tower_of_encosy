using System;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct TypeSpec : IEquatable<TypeSpec>
    {
        public string fullName;
        public string simpleName;
        public bool isValueType;
        public bool hasParameterlessConstructor;

        public readonly bool IsValid => string.IsNullOrEmpty(fullName) == false;

        public readonly bool Equals(TypeSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && string.Equals(simpleName, other.simpleName, StringComparison.Ordinal)
            && isValueType == other.isValueType
            && hasParameterlessConstructor == other.hasParameterlessConstructor
            ;

        public readonly override bool Equals(object obj)
            => obj is TypeSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName, simpleName, isValueType, hasParameterlessConstructor);
    }
}
