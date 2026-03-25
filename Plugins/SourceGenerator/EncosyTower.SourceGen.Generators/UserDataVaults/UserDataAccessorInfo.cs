using System;
using System.Collections.Immutable;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    /// <summary>
    /// Cache-friendly, equatable representation of one constructor parameter of a
    /// <c>[UserDataAccessor]</c> class, extracted during the incremental pipeline transform.
    /// </summary>
    internal readonly struct AccessorArgInfo : IEquatable<AccessorArgInfo>
    {
        /// <summary>
        /// <see langword="true"/> if this parameter is a concrete <c>UserDataStoreBase&lt;T&gt;</c>;
        /// <see langword="false"/> if it is an <c>IUserDataAccessor</c> implementer.
        /// </summary>
        public readonly bool IsStore;

        /// <summary>Fully-qualified name of the store or accessor type (includes <c>global::</c>).</summary>
        public readonly string FullTypeName;

        /// <summary>
        /// Fully-qualified name of the data type <c>T</c> when <see cref="IsStore"/> is
        /// <see langword="true"/>; empty string otherwise.
        /// </summary>
        public readonly string FullDataTypeName;

        /// <summary>
        /// Simple (unqualified) name used in generated code:
        /// when <see cref="IsStore"/> is <see langword="true"/>, the data type <c>T</c> simple name
        /// (used for store field ordering and naming); otherwise, the accessor type simple name
        /// (used as the constructor argument in scaffolded initialization code).
        /// </summary>
        public readonly string TypeName;

        /// <summary>
        /// When <see cref="IsStore"/> is <see langword="true"/>, whether the data type <c>T</c> has
        /// a default (parameterless) constructor. Controls whether a <c>Create{T}()</c> partial
        /// method must be generated instead of <c>new T()</c>.
        /// </summary>
        public readonly bool DataTypeHasDefaultConstructor;

        public AccessorArgInfo(
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

        public readonly bool Equals(AccessorArgInfo other)
            => IsStore == other.IsStore
            && string.Equals(FullTypeName, other.FullTypeName, StringComparison.Ordinal)
            && string.Equals(FullDataTypeName, other.FullDataTypeName, StringComparison.Ordinal)
            && string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && DataTypeHasDefaultConstructor == other.DataTypeHasDefaultConstructor;

        public readonly override bool Equals(object obj)
            => obj is AccessorArgInfo other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(IsStore, FullTypeName, FullDataTypeName, TypeName)
                .Add(DataTypeHasDefaultConstructor);
    }

    /// <summary>
    /// Cache-friendly, equatable data extracted from a <c>[UserDataAccessor]</c>-attributed class
    /// during the incremental source-generation pipeline.
    /// </summary>
    internal struct UserDataAccessorInfo : IEquatable<UserDataAccessorInfo>
    {
        /// <summary>Excluded from equality/hash — location is not stable across incremental runs.</summary>
        public LocationInfo location;

        /// <summary>
        /// CLR metadata name of the accessor class, used as the equality key.
        /// </summary>
        public string metadataName;

        /// <summary>
        /// CLR metadata name of the target vault type from the attribute's first constructor argument;
        /// <see langword="null"/> or empty when the accessor applies to all vaults.
        /// </summary>
        public string vaultMetadataName;

        /// <summary>
        /// Display name derived from a <c>[Label]</c> or <c>[DisplayName]</c> attribute on the
        /// accessor class, or the class's own <c>Name</c> as a fallback.
        /// </summary>
        public string fieldName;

        /// <summary>
        /// Simple (unqualified) class name, used in generated constructor-assignment code.
        /// </summary>
        public string symbolName;

        /// <summary>
        /// Pre-extracted constructor argument data for cache equality and diagnostic pre-filtering.
        /// </summary>
        public EquatableArray<AccessorArgInfo> args;

        /// <summary>
        /// <see langword="true"/> when the accessor class implements
        /// <c>global::EncosyTower.Initialization.IInitializable</c>.
        /// </summary>
        public bool isInitializable;

        /// <summary>
        /// <see langword="true"/> when the accessor class implements
        /// <c>global::EncosyTower.Initialization.IDeinitializable</c>.
        /// </summary>
        public bool isDeinitializable;

        /// <summary>
        /// <see langword="false"/> when validation found no usable constructor or unsupported
        /// parameter types; such items are filtered out before code generation.
        /// </summary>
        public bool isValid;

        public readonly bool Equals(UserDataAccessorInfo other)
            => string.Equals(metadataName, other.metadataName, StringComparison.Ordinal)
            && string.Equals(vaultMetadataName, other.vaultMetadataName, StringComparison.Ordinal)
            && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
            && string.Equals(symbolName, other.symbolName, StringComparison.Ordinal)
            && isInitializable == other.isInitializable
            && isDeinitializable == other.isDeinitializable
            && isValid == other.isValid
            && args.Equals(other.args);

        public readonly override bool Equals(object obj)
            => obj is UserDataAccessorInfo other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(metadataName, vaultMetadataName, fieldName, symbolName)
                .Add(isInitializable)
                .Add(isDeinitializable)
                .Add(isValid)
                .Add(args.GetHashCode());
    }
}
