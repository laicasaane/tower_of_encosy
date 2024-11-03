using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    using static EncosyTower.Modules.DataAuthoring.SourceGen.Helpers;

    public partial class DatabaseDeclaration
    {
        public DatabaseRef DatabaseRef { get; }

        public DatabaseDeclaration(SourceProductionContext context, DatabaseRef databaseRef)
        {
            DatabaseRef = databaseRef;
            InitializeNamingStrategy();
            InitializeConverters(context);
            InitializeTables(context);
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
                INamedTypeSymbol type;

                if (member is IFieldSymbol field)
                {
                    type = field.Type as INamedTypeSymbol;
                }
                else if (member is IPropertySymbol property)
                {
                    type = property.Type as INamedTypeSymbol;
                }
                else
                {
                    continue;
                }

                if (type == null)
                {
                    continue;
                }

                var attrib = member.GetAttribute(TABLE_ATTRIBUTE);

                if (attrib == null)
                {
                    continue;
                }

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
                    Type = type,
                    BaseType = baseType,
                    SheetName = member.Name,
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

                tables.Add(table);

                GetHorizontalLists(context, databaseRef, member, table);
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
