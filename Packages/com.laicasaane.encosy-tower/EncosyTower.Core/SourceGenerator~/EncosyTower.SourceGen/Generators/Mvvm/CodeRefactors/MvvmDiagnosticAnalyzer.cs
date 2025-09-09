﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.Mvvm.CodeRefactors
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class MvvmDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_FIELD = "MVVMTK0010";
        public const string DIAGNOSTIC_PROPERTY = "MVVMTK0011";

        public const string INTERFACE = "EncosyTower.Mvvm.ComponentModel.IObservableObject";
        public const string ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.ObservablePropertyAttribute";

        private static readonly DiagnosticDescriptor s_diagnosticField = new(
              id: DIAGNOSTIC_FIELD
            , title: "Field can be replaced by a property"
            , messageFormat: "Field name '{0}' can be replaced by a property"
            , category: nameof(MvvmDiagnosticAnalyzer)
            , DiagnosticSeverity.Hidden
            , isEnabledByDefault: true
            , description: "Field can be replaced by a property."
        );

        private static readonly DiagnosticDescriptor s_diagnosticProperty = new(
              id: DIAGNOSTIC_PROPERTY
            , title: "Property can be replaced by a field"
            , messageFormat: "Property name '{0}' can be replaced by a field"
            , category: nameof(MvvmDiagnosticAnalyzer)
            , DiagnosticSeverity.Hidden
            , isEnabledByDefault: true
            , description: "Property can be replaced by a field."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(s_diagnosticField, s_diagnosticProperty);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        private void AnalyzeField(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IFieldSymbol fieldSymbol
                || fieldSymbol.ContainingType is not INamedTypeSymbol typeSymbol
                || typeSymbol.InheritsFromInterface(INTERFACE) == false
                || fieldSymbol.HasAttribute(ATTRIBUTE) == false
            )
            {
                return;
            }

            var diagnostic = Diagnostic.Create(s_diagnosticField, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private void AnalyzeProperty(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IPropertySymbol propSymbol
                || propSymbol.ContainingType is not INamedTypeSymbol typeSymbol
                || typeSymbol.InheritsFromInterface(INTERFACE) == false
                || propSymbol.HasAttribute(ATTRIBUTE) == false
            )
            {
                return;
            }

            var diagnostic = Diagnostic.Create(s_diagnosticProperty, propSymbol.Locations[0], propSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
