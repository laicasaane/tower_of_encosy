using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    /// <summary>
    /// Validates types annotated with <c>[NewtonsoftJsonAotHelper]</c> and reports
    /// actionable diagnostics.
    /// <para>
    /// Diagnostics are deliberately kept in a <see cref="DiagnosticAnalyzer"/> rather
    /// than emitted from the source generator itself.  This prevents unnecessary
    /// source-file regeneration when only an invalid attribute usage changes.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class NewtonsoftJsonAotHelperAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Serialization.NewtonsoftJson";
        private const string ATTRIBUTE = $"global::{NAMESPACE}.NewtonsoftJsonAotHelperAttribute";

        public static readonly DiagnosticDescriptor MustNotBeAbstract = new(
              id: "NEWTONSOFT_AOT_HELPER_0001"
            , title: "[NewtonsoftJsonAotHelper] cannot be applied to an abstract type"
            , messageFormat: "\"{0}\" is abstract. [NewtonsoftJsonAotHelper] requires a non-abstract, concrete type."
            , category: "NewtonsoftJsonAotHelperGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "[NewtonsoftJsonAotHelper] is not supported on abstract types."
        );

        public static readonly DiagnosticDescriptor MustNotBeUnboundGeneric = new(
              id: "NEWTONSOFT_AOT_HELPER_0002"
            , title: "[NewtonsoftJsonAotHelper] cannot be applied to an unbound generic type"
            , messageFormat: "\"{0}\" is an unbound generic type. [NewtonsoftJsonAotHelper] requires a closed, non-generic type."
            , category: "NewtonsoftJsonAotHelperGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "[NewtonsoftJsonAotHelper] is not supported on unbound-generic types."
        );

        public static readonly DiagnosticDescriptor BaseTypeMustBeProvided = new(
              id: "NEWTONSOFT_AOT_HELPER_0003"
            , title: "[NewtonsoftJsonAotHelper] requires a valid base type argument"
            , messageFormat: "\"{0}\" is annotated with [NewtonsoftJsonAotHelper] but the required base-type constructor argument is missing or is not a type symbol."
            , category: "NewtonsoftJsonAotHelperGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "[NewtonsoftJsonAotHelper(typeof(T))] requires exactly one typeof(T) argument specifying the base type to scan for."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustNotBeAbstract
                , MustNotBeUnboundGeneric
                , BaseTypeMustBeProvided
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(ATTRIBUTE) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(ATTRIBUTE);
            var location = attrib?.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                ?? typeSymbol.Locations[0];

            if (typeSymbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeAbstract
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            if (typeSymbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeUnboundGeneric
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            var args = attrib?.ConstructorArguments ?? default;

            if (args.IsDefaultOrEmpty
                || args.Length != 1
                || args[0].Value is not ITypeSymbol
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      BaseTypeMustBeProvided
                    , location
                    , typeSymbol.Name
                ));
            }
        }
    }
}
