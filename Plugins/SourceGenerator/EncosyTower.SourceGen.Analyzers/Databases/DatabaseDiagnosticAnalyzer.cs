using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Databases
{
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
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(DATABASE_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property
                    || property.Type is not INamedTypeSymbol propType
                )
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE, token);

                if (tableAttrib == null)
                {
                    continue;
                }

                if (TryReportTablePropertyType(context, member, propType, tableAttrib, token))
                {
                    continue;
                }

                ValidateHorizontalAttributes(context, member, token);
            }
        }

        private static bool TryReportTablePropertyType(
              SymbolAnalysisContext context
            , ISymbol member
            , INamedTypeSymbol propType
            , AttributeData tableAttrib
            , CancellationToken token
        )
        {
            var memberLocation = member.Locations.Length > 0
                ? member.Locations[0]
                : Location.None;

            var attribLocation = tableAttrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? memberLocation;

            if (propType.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TableDiagnosticDescriptors.AbstractTypeNotSupported
                    , attribLocation
                    , propType.Name
                ));
                return true;
            }

            if (propType.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TableDiagnosticDescriptors.GenericTypeNotSupported
                    , attribLocation
                    , propType.Name
                ));
                return true;
            }

            if (propType.BaseType == null
                || propType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out _, token) == false
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TableDiagnosticDescriptors.MustBeDerivedFromDataTableAsset
                    , attribLocation
                    , propType.Name
                ));
                return true;
            }

            return false;
        }

        private static void ValidateHorizontalAttributes(
              SymbolAnalysisContext context
            , ISymbol member
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var attributes = member.GetAttributes(HORIZONTAL_LIST_ATTRIBUTE, token);

            foreach (var attrib in attributes)
            {
                token.ThrowIfCancellationRequested();

                var args = attrib.ConstructorArguments;

                if (args.Length < 2)
                {
                    continue;
                }

                var attribLocation = attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
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

                if (targetType.HasAttribute(DATA_ATTRIBUTE, token) == false
                    && targetType.InheritsFromInterface(IDATA, true, token) == false
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
