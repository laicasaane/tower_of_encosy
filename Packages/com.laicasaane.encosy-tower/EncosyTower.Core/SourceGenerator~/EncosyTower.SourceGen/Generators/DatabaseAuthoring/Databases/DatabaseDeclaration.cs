using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

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
            var attrib = DatabaseRef.DatabaseAttribute;
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
            var attrib = DatabaseRef.DatabaseAttribute;
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
            var members = databaseRef.DatabaseSymbol.GetMembers();
            var outerNode = databaseRef.Syntax;
            var namingStrategy = databaseRef.NamingStrategy;
            var token = context.CancellationToken;

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

                var syntaxNode = property.DeclaringSyntaxReferences[0].GetSyntax(token);

                if (type.BaseType == null
                    || type.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out var baseType) == false
                )
                {
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

                GetHorizontalLists(databaseRef, member, table);

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
              DatabaseRef databaseRef
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
                    continue;
                }

                if (args[1].Value is not string propertyName || string.IsNullOrWhiteSpace(propertyName))
                {
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

        public static string GetUniqueSheetName(TableRef table)
            => GetUniqueSheetName(table.Type, table.DataType, table.PropertyName);

        public static string GetSheetName(TableRef table)
            => GetSheetName(table.Type, table.DataType);

        public static string GetUniqueSheetName(INamedTypeSymbol tableType, ITypeSymbol dataType, string memberName)
            => $"{GetSheetName(tableType, dataType)}__{memberName}";

        public static string GetSheetName(INamedTypeSymbol tableType, ITypeSymbol dataType)
            => $"{tableType.Name}_{dataType.Name}Sheet";

        public static string GetTableAssetName(TableRef table)
            => $"{table.Type.Name}_{table.PropertyName}";
    }
}
