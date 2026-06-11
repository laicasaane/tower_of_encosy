using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.DataTableAssets
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class DataTableAssetAnalyzer : DiagnosticAnalyzer
    {
        private const string DATA_TABLE_ASSET = "global::EncosyTower.Databases.DataTableAsset";
        private const string DATA_TABLE_ASSET_ATTRIBUTE = "EncosyTower.Databases.DataTableAssetAttribute";

        public static readonly DiagnosticDescriptor MustBeApplicableForTypeArgument = new(
              id: "SG_DATA_TABLE_ASSET_0001"
            , title: "Must be either a struct, a class or an enum to replace type argument"
            , messageFormat: "Type \"{0}\" is not applicable to replace \"{1}\", must be either a struct, a class or an enum"
            , category: "DataTableAssetGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Must be either a struct, a class or an enum."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MustBeApplicableForTypeArgument);

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

            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            var hasAttribute = false;

            foreach (var attr in typeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attrClass = attr.AttributeClass;

                if (attrClass is null)
                {
                    continue;
                }

                if (attrClass.HasFullName(DATA_TABLE_ASSET_ATTRIBUTE, token))
                {
                    hasAttribute = true;
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            if (hasAttribute == false)
            {
                return;
            }

            var baseType = typeSymbol.BaseType;

            while (baseType != null)
            {
                token.ThrowIfCancellationRequested();

                var typeArguments = baseType.TypeArguments;

                if (typeArguments.Length >= 2)
                {
                    if (baseType.HasFullNamePrefix(DATA_TABLE_ASSET, token))
                    {
                        var idType = typeArguments[0];
                        var dataType = typeArguments[1];
                        var typeLocation = typeSymbol.Locations.Length > 0
                            ? typeSymbol.Locations[0]
                            : Location.None;

                        if (idType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  MustBeApplicableForTypeArgument
                                , typeLocation
                                , idType.Name
                                , "TDataId"
                            ));
                        }

                        if (dataType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  MustBeApplicableForTypeArgument
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
