using System;

namespace EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers
{
    internal enum AotCollectionKind
    {
        None,
        List,
        HashSet,
        Dictionary,
    }

    internal struct AotTypeArgSpec : IEquatable<AotTypeArgSpec>
    {
        public string fullName;
        public bool canEnsure;

        public readonly bool Equals(AotTypeArgSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && canEnsure == other.canEnsure;

        public readonly override bool Equals(object obj)
            => obj is AotTypeArgSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName).Add(canEnsure);
    }

    internal struct AotFieldSpec : IEquatable<AotFieldSpec>
    {
        public string fieldTypeFullName;
        public bool fieldTypeCanEnsure;
        public AotCollectionKind collectionKind;
        public AotTypeArgSpec elementOrKey;
        public AotTypeArgSpec dictionaryValue;
        public EquatableArray<AotTypeArgSpec> otherTypeArgs;

        public readonly bool Equals(AotFieldSpec other)
            => string.Equals(fieldTypeFullName, other.fieldTypeFullName, StringComparison.Ordinal)
            && fieldTypeCanEnsure == other.fieldTypeCanEnsure
            && collectionKind == other.collectionKind
            && elementOrKey.Equals(other.elementOrKey)
            && dictionaryValue.Equals(other.dictionaryValue)
            && otherTypeArgs.Equals(other.otherTypeArgs)
            ;

        public readonly override bool Equals(object obj)
            => obj is AotFieldSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fieldTypeFullName)
            .Add(fieldTypeCanEnsure)
            .Add(collectionKind)
            .Add(elementOrKey.GetHashCode())
            .Add(dictionaryValue.GetHashCode())
            .Add(otherTypeArgs.GetHashCode());
    }

    internal struct AotTypeSpec : IEquatable<AotTypeSpec>
    {
        public string fullName;
        public EquatableArray<AotFieldSpec> fields;

        public readonly bool IsValid => fullName is not null;

        public readonly bool Equals(AotTypeSpec other)
            => string.Equals(fullName, other.fullName, StringComparison.Ordinal)
            && fields.Equals(other.fields)
            ;

        public readonly override bool Equals(object obj)
            => obj is AotTypeSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(fullName)
            .Add(fields.GetHashCode());
    }
}
