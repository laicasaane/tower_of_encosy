using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.EnumExtensions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class EnumExtensionsAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string ENUM_EXTENSIONS_FOR_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsForAttribute";

        public static readonly DiagnosticDescriptor TypeArgumentMustBeEnum = new(
              id: "SG_ENUM_EXT_FOR_0001"
            , title: "Type argument of [EnumExtensionsFor] must be an enum type"
            , messageFormat: "\"{0}\" is not an enum type. The typeof argument of [EnumExtensionsFor] must resolve to an enum."
            , category: "EnumExtensionsForGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [EnumExtensionsFor(typeof(...))] must be an enum type."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(TypeArgumentMustBeEnum);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeStaticClass, SymbolKind.NamedType);
        }

        private static void AnalyzeStaticClass(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.IsStatic == false
                || typeSymbol.HasAttribute(ENUM_EXTENSIONS_FOR_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(ENUM_EXTENSIONS_FOR_ATTRIBUTE, token);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
            {
                return;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type)
            {
                return;
            }

            if (typeArg.Value is not INamedTypeSymbol argSymbol
                || argSymbol.TypeKind != TypeKind.Enum
            )
            {
                var displayName = (typeArg.Value as ISymbol)?.ToDisplayString() ?? typeArg.Value?.ToString() ?? "?";
                var location = attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      TypeArgumentMustBeEnum
                    , location
                    , displayName
                ));
            }
        }
    }
}
