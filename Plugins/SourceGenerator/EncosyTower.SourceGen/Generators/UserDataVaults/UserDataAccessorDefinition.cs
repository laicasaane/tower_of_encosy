using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    internal class UserDataAccessorDefinition
    {
        public bool IsValid { get; }

        public INamedTypeSymbol Symbol { get; }

        public string FullTypeName { get; }

        public string FieldName { get; }

        public List<ParamDefinition> Args { get; }

        public UserDataAccessorDefinition(
              SourceProductionContext context
            , ClassDeclarationSyntax providerSyntax
            , INamedTypeSymbol symbol
        )
        {
            Symbol = symbol;
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
                context.ReportDiagnostic(
                      DiagnosticDescriptors.MustHaveOnlyOneConstructor
                    , providerSyntax
                    , symbol.Name
                );
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
                        context.ReportDiagnostic(
                              DiagnosticDescriptors.ConstructorContainsUnsupportedType
                            , providerSyntax
                            , symbol.Name
                            , param.Name
                        );
                    }
                }

                IsValid = validCount == parameters.Length;
            }
        }
    }

    internal readonly struct ParamDefinition
    {
        public readonly StoreDefinition StoreDef;
        public readonly ITypeSymbol Type;

        public ParamDefinition(ITypeSymbol type, ITypeSymbol argType)
        {
            if (argType != null)
            {
                StoreDef = new StoreDefinition(type, argType);
                Type = default;
            }
            else
            {
                StoreDef = default;
                Type = type;
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
        public readonly ITypeSymbol StoreType;
        public readonly ITypeSymbol DataType;
        public readonly string FullStoreTypeName;
        public readonly string FullDataTypeName;
        public readonly string DataFieldName;
        public readonly string DataArgName;

        public bool IsValid => StoreType != null && DataType != null;

        public StoreDefinition(ITypeSymbol storeType, ITypeSymbol dataType)
        {
            StoreType = storeType;
            DataType = dataType;
            FullStoreTypeName = storeType.ToFullName();
            FullDataTypeName = dataType.ToFullName();
            DataFieldName = dataType.Name.ToPrivateFieldName();
            DataArgName = dataType.Name.ToArgumentName();
        }

        public bool Equals(StoreDefinition other)
            => SymbolEqualityComparer.Default.Equals(StoreType, other.StoreType);

        public override bool Equals(object obj)
            => obj is StoreDefinition other && Equals(other);

        public override int GetHashCode()
            => SymbolEqualityComparer.Default.GetHashCode(StoreType);
    }
}
