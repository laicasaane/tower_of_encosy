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
            InitializeTables(context);
        }

        private void InitializeAssetName()
        {
            var attrib = DatabaseRef.Attribute;
            var args = attrib.NamedArguments;
            var databaseAssetName = string.Empty;
            var withInstanceAPI = false;

            foreach (var arg in args)
            {
                if (arg.Key == "AssetName"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value?.ToString() is string assetName
                )
                {
                    databaseAssetName = assetName;
                    continue;
                }

                if (arg.Key == "WithInstanceAPI"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value is bool withAPI
                )
                {
                    withInstanceAPI = withAPI;
                    continue;
                }
            }

            DatabaseRef.AssetName = string.IsNullOrWhiteSpace(databaseAssetName)
                ? $"DatabaseAsset_{DatabaseRef.Syntax.Identifier.Text}"
                : databaseAssetName;

            DatabaseRef.WithInstanceAPI = withInstanceAPI;
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

        private void InitializeTables(SourceProductionContext context)
        {
            var tables = new List<TableRef>();
            var databaseRef = DatabaseRef;
            var members = databaseRef.Symbol.GetMembers();
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
                    || type.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out _) == false
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
                    PropertyName = property.Name,
                    NamingStrategy = namingStrategy,
                };

                ValidateHorizontalLists(context, member);

                tables.Add(table);
            }

            if (tables.Count > 0)
            {
                using var arrayBuilder = ImmutableArrayBuilder<TableRef>.Rent();
                arrayBuilder.AddRange(tables);
                databaseRef.SetTables(arrayBuilder.ToImmutable());
            }
        }

        private static void ValidateHorizontalLists(SourceProductionContext context, ISymbol member)
        {
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
            }
        }
    }
}
