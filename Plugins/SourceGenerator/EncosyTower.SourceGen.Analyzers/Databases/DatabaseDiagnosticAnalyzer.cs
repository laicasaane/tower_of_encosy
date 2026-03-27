using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Analyzers.Databases
{
    /// <summary>
    /// Analyzes types marked with <c>[Database]</c> and reports validation errors that are
    /// intentionally excluded from the source generator to keep the incremental pipeline
    /// cache-friendly.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DatabaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        private const string DATABASES_AUTHORING_NAMESPACE = DATABASES_NAMESPACE + ".Authoring";
        private const string DATA_NAMESPACE = "EncosyTower.Data";

        private const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        private const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";
        private const string HORIZONTAL_LIST_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.HorizontalAttribute";
        private const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        private const string DATA_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataAttribute";
        private const string IDATA = $"global::{DATA_NAMESPACE}.IData";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  TableDiagnosticDescriptors.NotTypeOfExpression
                , TableDiagnosticDescriptors.AbstractTypeNotSupported
                , TableDiagnosticDescriptors.GenericTypeNotSupported
                , TableDiagnosticDescriptors.MustBeDerivedFromDataTableAsset
                , HorizontalDiagnosticDescriptors.NotTypeOfExpression
                , HorizontalDiagnosticDescriptors.AbstractTypeNotSupported
                , HorizontalDiagnosticDescriptors.NotImplementIData
                , HorizontalDiagnosticDescriptors.InvalidPropertyName
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            // Gate on [Database] attribute presence
            if (typeSymbol.HasAttribute(DATABASE_ATTRIBUTE) == false)
            {
                return;
            }

            var members = typeSymbol.GetMembers();

            foreach (var member in members)
            {
                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol propType)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE);

                if (tableAttrib == null)
                {
                    continue;
                }

                var attribLocation = tableAttrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? (member.Locations.Length > 0 ? member.Locations[0] : Location.None);

                if (propType.IsAbstract)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          TableDiagnosticDescriptors.AbstractTypeNotSupported
                        , attribLocation
                        , propType.Name
                    ));
                    continue;
                }

                if (propType.IsGenericType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          TableDiagnosticDescriptors.GenericTypeNotSupported
                        , attribLocation
                        , propType.Name
                    ));
                    continue;
                }

                if (propType.BaseType == null
                    || propType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out _) == false
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          TableDiagnosticDescriptors.MustBeDerivedFromDataTableAsset
                        , attribLocation
                        , propType.Name
                    ));
                    continue;
                }

                // Also validate [Horizontal] attributes on the same property
                ValidateHorizontalAttributes(context, member);
            }
        }

        private static void ValidateHorizontalAttributes(SymbolAnalysisContext context, ISymbol member)
        {
            var attributes = member.GetAttributes(HORIZONTAL_LIST_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                var args = attrib.ConstructorArguments;

                if (args.Length < 2)
                {
                    continue;
                }

                var attribLocation = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? (member.Locations.Length > 0 ? member.Locations[0] : Location.None);

                if (args[0].Value is not INamedTypeSymbol targetType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          HorizontalDiagnosticDescriptors.NotTypeOfExpression
                        , attribLocation
                    ));
                    continue;
                }

                if (targetType.IsAbstract)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          HorizontalDiagnosticDescriptors.AbstractTypeNotSupported
                        , attribLocation
                        , targetType.Name
                    ));
                    continue;
                }

                if (targetType.HasAttribute(DATA_ATTRIBUTE) == false
                    && targetType.InheritsFromInterface(IDATA) == false
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          HorizontalDiagnosticDescriptors.NotImplementIData
                        , attribLocation
                        , targetType.Name
                    ));
                    continue;
                }

                if (args[1].Value is not string propertyName || string.IsNullOrWhiteSpace(propertyName))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          HorizontalDiagnosticDescriptors.InvalidPropertyName
                        , attribLocation
                    ));
                    continue;
                }
            }
        }
    }
}
