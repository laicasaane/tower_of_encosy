using System;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal readonly struct AccessorArgSpec : IEquatable<AccessorArgSpec>
    {
        public readonly bool IsStore;
        public readonly string FullTypeName;
        public readonly string FullDataTypeName;
        public readonly string TypeName;
        public readonly bool DataTypeHasDefaultConstructor;

        public AccessorArgSpec(
              bool isStore
            , string fullTypeName
            , string fullDataTypeName
            , string typeName
            , bool dataTypeHasDefaultConstructor
        )
        {
            IsStore = isStore;
            FullTypeName = fullTypeName;
            FullDataTypeName = fullDataTypeName;
            TypeName = typeName;
            DataTypeHasDefaultConstructor = dataTypeHasDefaultConstructor;
        }

        public readonly bool Equals(AccessorArgSpec other)
            => IsStore == other.IsStore
            && string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal)
            && string.Equals(FullDataTypeName, other.FullDataTypeName, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && DataTypeHasDefaultConstructor == other.DataTypeHasDefaultConstructor;

        public readonly override bool Equals(object obj)
            => obj is AccessorArgSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(IsStore, FullTypeName, FullDataTypeName, TypeName)
                .Add(DataTypeHasDefaultConstructor);
    }

    internal struct UserDataAccessorSpec : IEquatable<UserDataAccessorSpec>
    {
        public LocationInfo location;
        public string metadataName;
        public string vaultMetadataName;
        public string fieldName;
        public string symbolName;
        public EquatableArray<AccessorArgSpec> args;
        public bool isInitializable;
        public bool isDeinitializable;
        public bool isValid;

        public readonly bool Equals(UserDataAccessorSpec other)
            => string.Equals(metadataName, other.metadataName, StringComparison.Ordinal)
            && string.Equals(vaultMetadataName, other.vaultMetadataName, StringComparison.Ordinal)
            && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
            && string.Equals(symbolName, other.symbolName, StringComparison.Ordinal)
            && isInitializable == other.isInitializable
            && isDeinitializable == other.isDeinitializable
            && isValid == other.isValid
            && args.Equals(other.args);

        public readonly override bool Equals(object obj)
            => obj is UserDataAccessorSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(metadataName, vaultMetadataName, fieldName, symbolName)
                .Add(isInitializable)
                .Add(isDeinitializable)
                .Add(isValid)
                .Add(args.GetHashCode());
    }
}
