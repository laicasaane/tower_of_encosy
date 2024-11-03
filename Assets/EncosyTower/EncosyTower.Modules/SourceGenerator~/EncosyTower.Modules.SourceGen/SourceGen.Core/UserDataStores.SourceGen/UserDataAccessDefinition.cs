using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UserDataStores.SourceGen
{
    internal class UserDataAccessDefinition
    {
        public bool IsValid { get; }

        public INamedTypeSymbol Symbol { get; }

        public string FieldName { get; }

        public List<ParamDefinition> Args { get; }

        public UserDataAccessDefinition(
              SourceProductionContext context
            , ClassDeclarationSyntax providerSyntax
            , INamedTypeSymbol symbol
            , string prefix
            , string suffix
            , StringBuilder sb
        )
        {
            Symbol = symbol;

            var className = symbol.Name;
            var classNameSpan = className.AsSpan();
            var classNameLength = classNameSpan.Length;

            sb.Clear();
            sb.Append(className);

            if (string.IsNullOrEmpty(prefix) == false)
            {
                var index = classNameSpan.IndexOf(prefix.AsSpan(), StringComparison.Ordinal);

                if (index == 0)
                {
                    sb.Remove(0, prefix.Length);
                    classNameLength -= prefix.Length;
                }
            }

            if (string.IsNullOrEmpty(suffix) == false)
            {
                var index = classNameSpan.IndexOf(suffix.AsSpan(), StringComparison.Ordinal);
                var lastIndex = classNameLength - suffix.Length;

                if (index == lastIndex)
                {
                    sb.Remove(0, suffix.Length);
                }
            }

            FieldName = sb.ToString();
            sb.Clear();

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
                    , className
                );
                return;
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
                            , className
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
        public const string USER_DATA_STORAGE = "global::EncosyTower.Modules.UserDataStores.UserDataStorage<";
        private const string IUSER_DATA_ACCESS = "global::EncosyTower.Modules.UserDataStores.IUserDataAccess";

        public readonly StorageDefinition StorageDef;
        public readonly ITypeSymbol Type;

        public ParamDefinition(ITypeSymbol type, ITypeSymbol argType)
        {
            if (argType != null)
            {
                StorageDef = new StorageDefinition(type, argType);
                Type = default;
            }
            else
            {
                StorageDef = default;
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
                && namedType.ToFullName().AsSpan().StartsWith(USER_DATA_STORAGE.AsSpan())
            )
            {
                var args = namedType.TypeArguments;

                if (args.Length == 1 && args[0].IsAbstract == false)
                {
                    argType = args[0];
                    return true;
                }

                return false;
            }

            return type.Interfaces.DoesMatchInterface(IUSER_DATA_ACCESS)
                || type.AllInterfaces.DoesMatchInterface(IUSER_DATA_ACCESS);
        }
    }

    internal readonly struct StorageDefinition : IEquatable<StorageDefinition>
    {
        public readonly ITypeSymbol StorageType;
        public readonly ITypeSymbol DataType;

        public bool IsValid => StorageType != null && DataType != null;

        public StorageDefinition(ITypeSymbol storageType, ITypeSymbol dataType)
        {
            StorageType = storageType;
            DataType = dataType;
        }

        public bool Equals(StorageDefinition other)
            => SymbolEqualityComparer.Default.Equals(StorageType, other.StorageType);

        public override bool Equals(object obj)
            => obj is StorageDefinition other && Equals(other);

        public override int GetHashCode()
            => SymbolEqualityComparer.Default.GetHashCode(StorageType);
    }
}
