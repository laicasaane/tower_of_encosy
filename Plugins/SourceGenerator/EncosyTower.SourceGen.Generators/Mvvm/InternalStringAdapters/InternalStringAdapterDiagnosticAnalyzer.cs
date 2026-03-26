using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.Mvvm.CodeRefactors
{
    /// <summary>
    /// Reports a warning when a method decorated with <c>[RelayCommand]</c> or
    /// <c>[Binding]</c> does not have exactly one parameter, because the
    /// <c>InternalStringAdapterGenerator</c> only generates adapters for single-parameter
    /// methods and will silently skip methods with any other parameter count.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class InternalStringAdapterDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "ISA_0001";

        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";
        public const string BINDING_PROPERTY_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.BindingPropertyAttribute";

        private static readonly DiagnosticDescriptor s_diagnostic = new(
              id: DIAGNOSTIC_ID
            , title: "Method with [RelayCommand] or [Binding] should have exactly one parameter"
            , messageFormat: "Method '{0}' is decorated with [{1}] but has {2} parameter(s). "
                + "The InternalStringAdapter generator only produces string adapters for methods with exactly one parameter; "
                + "this method will be skipped."
            , category: nameof(InternalStringAdapterDiagnosticAnalyzer)
            , DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The InternalStringAdapter generator extracts the parameter type of methods "
                + "decorated with [RelayCommand] or [Binding] to generate internal string adapters. "
                + "Methods with a parameter count other than one are silently skipped."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(s_diagnostic);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IMethodSymbol methodSymbol)
            {
                return;
            }

            string attributeShortName = null;

            if (methodSymbol.HasAttribute(RELAY_COMMAND_ATTRIBUTE))
            {
                attributeShortName = "RelayCommand";
            }
            else if (methodSymbol.HasAttribute(BINDING_PROPERTY_ATTRIBUTE))
            {
                attributeShortName = "BindingProperty";
            }

            if (attributeShortName is null)
            {
                return;
            }

            if (methodSymbol.Parameters.Length == 1)
            {
                // Exactly one parameter — the generator will handle this method.
                return;
            }

            var location = methodSymbol.Locations.Length > 0
                ? methodSymbol.Locations[0]
                : Location.None;

            context.ReportDiagnostic(Diagnostic.Create(
                  s_diagnostic
                , location
                , methodSymbol.Name
                , attributeShortName
                , methodSymbol.Parameters.Length
            ));
        }
    }
}
