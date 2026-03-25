using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    internal class UserDataAccessorDefinition
    {
        public bool IsValid { get; }

        /// <summary>Simple (unqualified) class name. Used in generated constructor-assignment code.</summary>
        public string SymbolName { get; }

        public string FullTypeName { get; }

        public string FieldName { get; }

        public bool IsInitializable { get; }

        public bool IsDeinitializable { get; }

        public List<ParamDefinition> Args { get; }

        /// <summary>
        /// Cache-friendly constructor: builds the definition purely from pre-extracted
        /// <see cref="UserDataAccessorInfo"/> data — no symbol or syntax access.
        /// </summary>
        public UserDataAccessorDefinition(UserDataAccessorInfo info)
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
                args.Add(new ParamDefinition(arg));
                validCount += 1;
            }

            IsValid = info.isValid && validCount == infoArgs.Count;
        }

        /// <summary>
        /// Symbol-based constructor retained for use by <see cref="UserDataVaultDiagnosticAnalyzer"/>
        /// (which operates at analysis time and has live symbol access).
        /// </summary>
        public UserDataAccessorDefinition(INamedTypeSymbol symbol)
        {
            SymbolName = symbol.Name;
            FullTypeName = symbol.ToFullName();

            foreach (var attribute in symbol.GetAttributes())
            {
                var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                if (attributeName is not ("LabelAttribute" or "DisplayNameAttribute"))
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length > 0)
                {
                    var arg = attribute.ConstructorArguments[0];

                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value?.ToString() is string dn)
                    {
                        FieldName = dn;
                        goto NEXT;
                    }
                }
                else if (attribute.NamedArguments.Length > 0)
                {
                    foreach (var arg in attribute.NamedArguments)
                    {
                        if (arg.Key is "Name" or "DisplayName"
                            && arg.Value.Kind == TypedConstantKind.Primitive
                            && arg.Value.Value?.ToString() is string dn
                        )
                        {
                            FieldName = dn;
                            goto NEXT;
                        }
                    }
                }
            }

            NEXT:

            if (string.IsNullOrEmpty(FieldName))
            {
                FieldName = symbol.Name;
            }

            IsInitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable");
            IsDeinitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable");

            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                var constructor = constructors[i];

                if (constructor.Parameters.Length > max)
                {
                    max = constructor.Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                // Invalid — no constructor or best constructor is not at index 0.
                // Diagnostics are reported by UserDataVaultDiagnosticAnalyzer.
            }
            else
            {
                var constructor = constructors[constructorIndex];
                var parameters = constructor.Parameters;
                var args = Args = new(parameters.Length);
                var validCount = 0;

                foreach (var param in parameters)
                {
                    if (ParamDefinition.TryGetParam(param.Type, out var argType))
                    {
                        args.Add(new ParamDefinition(param.Type, argType));
                        validCount += 1;
                    }
                    else
                    {
                        // Unsupported param type — reported by UserDataVaultDiagnosticAnalyzer.
                    }
                }

                IsValid = validCount == parameters.Length;
            }
        }
    }

    internal readonly struct ParamDefinition
    {
        public readonly StoreDefinition StoreDef;

        /// <summary>
        /// Simple (unqualified) name of the <c>IUserDataAccessor</c> parameter type.
        /// Empty when this parameter is a <c>UserDataStoreBase&lt;T&gt;</c> store.
        /// </summary>
        public readonly string AccessorTypeName;

        /// <summary>Cache-friendly constructor from pre-extracted <see cref="AccessorArgInfo"/>.</summary>
        public ParamDefinition(AccessorArgInfo info)
        {
            if (info.IsStore)
            {
                StoreDef = new StoreDefinition(
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

        /// <summary>Symbol-based constructor for the legacy analysis-time path.</summary>
        public ParamDefinition(ITypeSymbol type, ITypeSymbol argType)
        {
            if (argType != null)
            {
                StoreDef = new StoreDefinition(type, argType);
                AccessorTypeName = string.Empty;
            }
            else
            {
                StoreDef = default;
                AccessorTypeName = type.Name;
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

    internal readonly struct StoreDefinition : IEquatable<StoreDefinition>
    {
        public readonly string FullStoreTypeName;
        public readonly string FullDataTypeName;
        public readonly string DataTypeName;
        public readonly string DataFieldName;
        public readonly string DataArgName;

        public bool IsValid => string.IsNullOrEmpty(FullStoreTypeName) == false
            && string.IsNullOrEmpty(DataTypeName) == false;

        public readonly bool DataTypeHasDefaultConstructor;

        /// <summary>Cache-friendly constructor from pre-extracted string data.</summary>
        public StoreDefinition(
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

        /// <summary>Symbol-based constructor for the legacy analysis-time path.</summary>
        public StoreDefinition(ITypeSymbol storeType, ITypeSymbol dataType)
        {
            FullStoreTypeName = storeType.ToFullName();
            FullDataTypeName = dataType.ToFullName();
            DataTypeName = dataType.Name;
            DataFieldName = dataType.Name.ToPrivateFieldName();
            DataArgName = dataType.Name.ToArgumentName();
            DataTypeHasDefaultConstructor = ComputeHasDefaultConstructor(dataType);
        }

        private static bool ComputeHasDefaultConstructor(ITypeSymbol dataType)
        {
            var nonDefaultCount = 0;
            var defaultCount = 0;

            foreach (var member in dataType.GetMembers())
            {
                if (member is Microsoft.CodeAnalysis.IMethodSymbol method
                    && method.MethodKind == Microsoft.CodeAnalysis.MethodKind.Constructor)
                {
                    if (method.Parameters.Length > 0)
                        nonDefaultCount++;
                    else
                        defaultCount++;
                }
            }

            return defaultCount > 0 || nonDefaultCount < 1;
        }

        public bool Equals(StoreDefinition other)
            => string.Equals(FullStoreTypeName, other.FullStoreTypeName, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => obj is StoreDefinition other && Equals(other);

        public override int GetHashCode()
            => FullStoreTypeName is null ? 0 : FullStoreTypeName.GetHashCode();
    }
}
