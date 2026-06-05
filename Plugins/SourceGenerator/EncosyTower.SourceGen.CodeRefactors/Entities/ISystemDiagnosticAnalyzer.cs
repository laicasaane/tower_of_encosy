using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.CodeRefactors.Entities
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ISystemDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ISYSTEM = "SG_ISYSTEM_0001";

        public const string ATTRIBUTE_METADATA = "EncosyTower.Entities.ISystemAttribute";

        private static readonly DiagnosticDescriptor s_diagnostic = new(
              id: DIAGNOSTIC_ISYSTEM
            , title: "ISystem type can be completed"
            , messageFormat: "Type '{0}' is annotated with [ISystem] and may need ISystem members"
            , category: nameof(ISystemDiagnosticAnalyzer)
            , DiagnosticSeverity.Hidden
            , isEnabledByDefault: true
            , description: "Type annotated with [ISystem] can be completed with lifecycle members."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(s_diagnostic);

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

            if (typeSymbol.TypeKind is not TypeKind.Class and not TypeKind.Struct)
            {
                return;
            }

            if (typeSymbol.HasAttribute(ATTRIBUTE_METADATA) == false)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                  s_diagnostic
                , typeSymbol.Locations[0]
                , typeSymbol.Name
            );

            context.ReportDiagnostic(diagnostic);
        }
    }
}
