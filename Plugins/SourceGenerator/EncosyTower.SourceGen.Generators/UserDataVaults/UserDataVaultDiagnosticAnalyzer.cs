using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    /// <summary>
    /// Roslyn diagnostic analyzer that validates <c>[UserDataAccessor]</c>-attributed classes,
    /// reporting issues that were previously reported inside the source generator itself.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UserDataVaultDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  DiagnosticDescriptors.MustHaveOnlyOneConstructor
                , DiagnosticDescriptors.ConstructorContainsUnsupportedType
                , DiagnosticDescriptors.MustNotBeAbstract
                , DiagnosticDescriptors.MustNotBeUnboundGenericType
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol symbol
                || symbol.GetAttribute(ACCESSOR_ATTRIBUTE) is null
            )
            {
                return;
            }

            var symbolLocation = symbol.Locations.Length > 0
                ? symbol.Locations[0]
                : Location.None;

            if (symbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DiagnosticDescriptors.MustNotBeAbstract
                    , symbolLocation
                    , symbol.Name
                ));
                return;
            }

            if (symbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DiagnosticDescriptors.MustNotBeUnboundGenericType
                    , symbolLocation
                    , symbol.Name
                ));
                return;
            }

            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length > max)
                {
                    max = constructors[i].Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      DiagnosticDescriptors.MustHaveOnlyOneConstructor
                    , symbolLocation
                    , symbol.Name
                ));

                return;
            }

            var constructor = constructors[constructorIndex];

            foreach (var param in constructor.Parameters)
            {
                if (ParamDefinition.TryGetParam(param.Type, out _) == false)
                {
                    var location = param.Locations.Length > 0
                        ? param.Locations[0]
                        : Location.None;

                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.ConstructorContainsUnsupportedType
                        , location
                        , symbol.Name
                        , param.Name
                    ));
                }
            }
        }
    }
}
