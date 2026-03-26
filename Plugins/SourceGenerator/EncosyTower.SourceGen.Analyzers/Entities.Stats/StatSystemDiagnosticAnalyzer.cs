using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for types annotated with
    /// <c>[StatSystem]</c>, covering conditions that would cause the source generator
    /// to silently produce no output.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StatSystemDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";

        public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
              id: "SG_STAT_SYSTEM_0001"
            , title: "[StatSystem] cannot be applied to a generic type"
            , messageFormat: "\"{0}\" is a generic type. [StatSystem] can only be applied to non-generic types."
            , category: "StatSystemGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatSystem] can only be applied to non-generic types."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(MustNotBeGeneric);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE) == false
            )
            {
                return;
            }

            if (typeSymbol.IsGenericType)
            {
                var location = typeSymbol.Locations.Length > 0
                    ? typeSymbol.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeGeneric
                    , location
                    , typeSymbol.Name
                ));
            }
        }
    }
}
