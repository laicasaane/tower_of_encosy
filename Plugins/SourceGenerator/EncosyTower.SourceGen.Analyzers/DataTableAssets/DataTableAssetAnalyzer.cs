using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.DataTableAssets
{
    /// <summary>
    /// Roslyn diagnostic analyzer that validates types attributed with <c>[DataTableAsset]</c>
    /// and reports issues that are intentionally excluded from the source generator itself to
    /// keep the incremental pipeline cache-friendly.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class DataTableAssetAnalyzer : DiagnosticAnalyzer
    {
        private const string DATA_TABLE_ASSET = "global::EncosyTower.Databases.DataTableAsset";
        private const string DATA_TABLE_ASSET_ATTRIBUTE = "EncosyTower.Databases.DataTableAssetAttribute";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticDescriptors.MustBeApplicableForTypeArgument);

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

            // Gate on [DataTableAssetAttribute] presence
            var hasAttribute = false;

            foreach (var attr in typeSymbol.GetAttributes())
            {
                var attrClass = attr.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                if (attrClass.HasFullName(DATA_TABLE_ASSET_ATTRIBUTE))
                {
                    hasAttribute = true;
                    break;
                }
            }

            if (hasAttribute == false)
            {
                return;
            }

            // Walk base type chain to find DataTableAsset<TId, TData[, TConv]>
            var baseType = typeSymbol.BaseType;

            while (baseType != null)
            {
                var typeArguments = baseType.TypeArguments;

                if (typeArguments.Length >= 2)
                {
                    if (baseType.HasFullNamePrefix(DATA_TABLE_ASSET))
                    {
                        var idType = typeArguments[0];
                        var dataType = typeArguments[1];
                        var typeLocation = typeSymbol.Locations.Length > 0
                            ? typeSymbol.Locations[0]
                            : Location.None;

                        if (idType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  DiagnosticDescriptors.MustBeApplicableForTypeArgument
                                , typeLocation
                                , idType.Name
                                , "TDataId"
                            ));
                        }

                        if (dataType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  DiagnosticDescriptors.MustBeApplicableForTypeArgument
                                , typeLocation
                                , dataType.Name
                                , "TData"
                            ));
                        }

                        return;
                    }
                }

                baseType = baseType.BaseType;
            }
        }
    }
}
