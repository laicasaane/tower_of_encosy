using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    public partial class DatabaseDeclaration
    {
        public DatabaseRef DatabaseRef { get; }

        public DatabaseDeclaration(SourceProductionContext context, DatabaseRef databaseRef)
        {
            DatabaseRef = databaseRef;
            InitializeAssetName();
            InitializeNamingStrategy();
            InitializeConverters(context);
            InitializeTables(context);
        }

        private void InitializeAssetName()
        {
            var attrib = DatabaseRef.Attribute;
            var args = attrib.NamedArguments;
            var databaseAssetName = string.Empty;

            foreach (var arg in args)
            {
                if (arg.Key == "AssetName"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value?.ToString() is string assetName
                )
                {
                    databaseAssetName = assetName;
                    break;
                }
            }

            DatabaseRef.AssetName = string.IsNullOrWhiteSpace(databaseAssetName)
                ? $"DatabaseAsset_{DatabaseRef.Syntax.Identifier.Text}"
                : databaseAssetName;
        }

        private void InitializeNamingStrategy()
        {
            var attrib = DatabaseRef.Attribute;
            var args = attrib.ConstructorArguments;

            foreach (var arg in args)
            {
                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    DatabaseRef.NamingStrategy = arg.Value.ToNamingStrategy();
                    break;
                }
            }
        }

        private void InitializeConverters(SourceProductionContext context)
        {
            var attrib = DatabaseRef.Attribute;
            var args = attrib.ConstructorArguments;

            foreach (var arg in args)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    arg.Values.MakeConverterMap(context, DatabaseRef.Syntax, attrib, DatabaseRef.ConverterMap, 0);
                    break;
                }
            }
        }

        private void InitializeTables(SourceProductionContext context)
        {
            var tables = new List<TableRef>();
            var databaseRef = DatabaseRef;
            var members = databaseRef.Symbol.GetMembers();
            var outerNode = databaseRef.Syntax;
            var namingStrategy = databaseRef.NamingStrategy;

            foreach (var member in members)
            {
                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol type)
                {
                    continue;
                }

                var attrib = member.GetAttribute(TABLE_ATTRIBUTE);

                if (attrib == null)
                {
                    continue;
                }

                if (property.DeclaringSyntaxReferences.Length < 1)
                {
                    continue;
                }

                var syntaxNode = property.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken);

                if (type.IsAbstract)
                {
                    context.ReportDiagnostic(
                          TableDiagnosticDescriptors.AbstractTypeNotSupported
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , type.Name
                    );
                    continue;
                }

                if (type.IsGenericType)
                {
                    context.ReportDiagnostic(
                          TableDiagnosticDescriptors.GenericTypeNotSupported
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , type.Name
                    );
                    continue;
                }

                if (type.BaseType == null
                    || type.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out var baseType) == false
                )
                {
                    context.ReportDiagnostic(
                          TableDiagnosticDescriptors.MustBeDerivedFromDataTableAsset
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , type.Name
                    );
                    continue;
                }

                var table = new TableRef {
                    SyntaxNode = syntaxNode,
                    Type = type,
                    BaseType = baseType,
                    PropertyName = property.Name,
                    NamingStrategy = namingStrategy,
                };

                foreach (var arg in attrib.ConstructorArguments)
                {
                    if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                    {
                        table.NamingStrategy = arg.Value.ToNamingStrategy();
                    }
                    else if (arg.Kind == TypedConstantKind.Array)
                    {
                        arg.Values.MakeConverterMap(context, outerNode, attrib, table.ConverterMap, 2);
                    }
                }

                GetHorizontalLists(context, databaseRef, member, table);

                tables.Add(table);
            }

            if (tables.Count > 0)
            {
                using var arrayBuilder = ImmutableArrayBuilder<TableRef>.Rent();
                arrayBuilder.AddRange(tables);
                databaseRef.SetTables(arrayBuilder.ToImmutable());
            }
        }

        private static void GetHorizontalLists(
              SourceProductionContext context
            , DatabaseRef databaseRef
            , ISymbol member
            , TableRef tableRef
        )
        {
            var horizontalListMap = databaseRef.HorizontalListMap;
            var attributes = member.GetAttributes(HORIZONTAL_LIST_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                var args = attrib.ConstructorArguments;

                if (args.Length < 2)
                {
                    continue;
                }

                if (args[0].Value is not INamedTypeSymbol targetType)
                {
                    context.ReportDiagnostic(
                          HorizontalDiagnosticDescriptors.NotTypeOfExpression
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                    );
                    continue;
                }

                if (targetType.IsAbstract)
                {
                    context.ReportDiagnostic(
                          HorizontalDiagnosticDescriptors.AbstractTypeNotSupported
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , targetType.Name
                    );
                    continue;
                }

                if (targetType.InheritsFromInterface(IDATA, true) == false)
                {
                    context.ReportDiagnostic(
                          HorizontalDiagnosticDescriptors.NotImplementIData
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , targetType.Name
                    );
                    continue;
                }

                if (args[1].Value is not string propertyName || string.IsNullOrWhiteSpace(propertyName))
                {
                    context.ReportDiagnostic(
                          HorizontalDiagnosticDescriptors.InvalidPropertyName
                        , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                    );
                    continue;
                }

                var tableType = tableRef.Type;

                if (horizontalListMap.TryGetValue(targetType, out var innerMap) == false)
                {
                    horizontalListMap[targetType] = innerMap = new(SymbolEqualityComparer.Default);
                }

                if (innerMap.TryGetValue(tableType, out var propertNames) == false)
                {
                    innerMap[tableType] = propertNames = new();
                }

                propertNames.Add(propertyName);
            }
        }
    }
}
