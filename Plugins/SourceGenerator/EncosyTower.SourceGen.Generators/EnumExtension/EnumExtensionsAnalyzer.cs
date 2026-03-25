using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for types annotated with
    /// <c>[EnumExtensionsFor]</c>.
    /// <para>
    /// Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator itself prevents unnecessary regeneration of source files when only
    /// an error (and not the surrounding valid code) has changed.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class EnumExtensionsAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string ENUM_EXTENSIONS_FOR_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsForAttribute";

        public static readonly DiagnosticDescriptor TypeArgumentMustBeEnum = new(
              id: "ENUM_EXT_FOR_0001"
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
            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.IsStatic == false
                || typeSymbol.HasAttribute(ENUM_EXTENSIONS_FOR_ATTRIBUTE) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(ENUM_EXTENSIONS_FOR_ATTRIBUTE);

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
                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
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
