using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    internal class UserDataAccessorDeclaration
    {
        public bool IsValid { get; }

        public string SymbolName { get; }

        public string FullTypeName { get; }

        public string FieldName { get; }

        public bool IsInitializable { get; }

        public bool IsDeinitializable { get; }

        public List<ParamDeclaration> Args { get; }

        public UserDataAccessorDeclaration(UserDataAccessorSpec info)
        {
            SymbolName = info.symbolName;
            FullTypeName = "global::" + info.metadataName;
            FieldName = info.fieldName;
            IsInitializable = info.isInitializable;
            IsDeinitializable = info.isDeinitializable;

            var infoArgs = info.args;
            var args = Args = new(infoArgs.Count);
            var validCount = 0;

            foreach (var arg in infoArgs)
            {
                args.Add(new ParamDeclaration(arg));
                validCount += 1;
            }

            IsValid = info.isValid && validCount == infoArgs.Count;
        }
    }

    internal readonly struct ParamDeclaration
    {
        public readonly StoreSpec StoreDef;
        public readonly string AccessorTypeName;

        public ParamDeclaration(AccessorArgSpec info)
        {
            if (info.IsStore)
            {
                StoreDef = new StoreSpec(
                      info.FullTypeName
                    , info.FullDataTypeName
                    , info.TypeName
                    , info.DataTypeHasDefaultConstructor
                );

                AccessorTypeName = string.Empty;
            }
            else
            {
                StoreDef = default;
                AccessorTypeName = info.TypeName;
            }
        }

        public static bool TryGetParam(ITypeSymbol type, out ITypeSymbol argType)
        {
            argType = default;

            if (type.IsAbstract)
            {
                return false;
            }

            if (type is INamedTypeSymbol namedType
                && namedType.TryGetGenericType(USER_DATA_STORE_BASE, 1, out var baseType)
                && baseType.TypeArguments.Length == 1
                && baseType.TypeArguments[0].IsAbstract == false
            )
            {
                argType = baseType.TypeArguments[0];
                return true;
            }

            return type.Interfaces.DoesMatchInterface(IUSER_DATA_ACCESSOR)
                || type.AllInterfaces.DoesMatchInterface(IUSER_DATA_ACCESSOR);
        }
    }

    internal readonly struct StoreSpec : IEquatable<StoreSpec>
    {
        public readonly string FullStoreTypeName;
        public readonly string FullDataTypeName;
        public readonly string DataTypeName;
        public readonly string DataFieldName;
        public readonly string DataArgName;

        public bool IsValid => string.IsNullOrEmpty(FullStoreTypeName) == false
            && string.IsNullOrEmpty(DataTypeName) == false;

        public readonly bool DataTypeHasDefaultConstructor;

        public StoreSpec(
              string fullStoreTypeName
            , string fullDataTypeName
            , string dataTypeName
            , bool dataTypeHasDefaultConstructor
        )
        {
            FullStoreTypeName = fullStoreTypeName;
            FullDataTypeName = fullDataTypeName;
            DataTypeName = dataTypeName;
            DataFieldName = dataTypeName.ToPrivateFieldName();
            DataArgName = dataTypeName.ToArgumentName();
            DataTypeHasDefaultConstructor = dataTypeHasDefaultConstructor;
        }

        public bool Equals(StoreSpec other)
            => string.Equals(FullStoreTypeName, other.FullStoreTypeName, StringComparison.Ordinal);
    }
}
