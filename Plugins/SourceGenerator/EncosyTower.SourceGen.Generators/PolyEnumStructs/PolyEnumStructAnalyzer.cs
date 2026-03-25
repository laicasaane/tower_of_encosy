using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for types annotated with
    /// <c>[PolyEnumStruct]</c>.
    /// <para>
    /// Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator itself prevents unnecessary regeneration of source files when only
    /// an error (and not the surrounding valid code) has changed.
    /// </para>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class PolyEnumStructAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string INTERFACE_NAME = "IEnumCase";
        private const string UNDEFINED_NAME = "Undefined";

        public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
              id: "POLY_ENUM_STRUCT_0001"
            , title: "[PolyEnumStruct] cannot be applied to a generic struct"
            , messageFormat: "\"{0}\" is a generic type. [PolyEnumStruct] is not supported on generic structs."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[PolyEnumStruct] cannot be applied to a struct that has type parameters."
        );

        public static readonly DiagnosticDescriptor MustHaveCaseStructs = new(
              id: "POLY_ENUM_STRUCT_0002"
            , title: "No case structs declared in [PolyEnumStruct]"
            , messageFormat: "\"{0}\" has no case structs. At least one nested struct (other than the implicit Undefined case) must be declared."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A [PolyEnumStruct] type must contain at least one nested case struct."
        );

        public static readonly DiagnosticDescriptor IEnumCaseMethodMustNotBeGeneric = new(
              id: "POLY_ENUM_STRUCT_0003"
            , title: "Generic method in IEnumCase interface is not supported"
            , messageFormat: "Method \"{0}\" declared on IEnumCase is generic. Generic methods in IEnumCase are not supported and will be ignored by the generator."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Generic methods declared inside the nested IEnumCase interface are not supported and will be dropped by the generator."
        );

        public static readonly DiagnosticDescriptor CaseStructMethodMustNotBeGeneric = new(
              id: "POLY_ENUM_STRUCT_0004"
            , title: "Generic method on case struct is not supported"
            , messageFormat: "Method \"{0}\" on case struct \"{1}\" is generic. Generic methods on case structs are not supported and will be ignored by the generator."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Generic methods declared on case structs nested inside a [PolyEnumStruct] type are not supported and will be dropped by the generator."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustNotBeGeneric
                , MustHaveCaseStructs
                , IEnumCaseMethodMustNotBeGeneric
                , CaseStructMethodMustNotBeGeneric
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
                || typeSymbol.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(POLY_ENUM_STRUCT_ATTRIBUTE);
            var location = attrib?.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                ?? typeSymbol.Locations[0];

            // POLY_ENUM_STRUCT_0001 — generic structs are not supported
            if (typeSymbol.TypeParameters.Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeGeneric
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            var parentName = typeSymbol.Name;
            var verboseUndefinedName = $"{parentName}_Undefined";
            var validCaseCount = 0;

            foreach (var nested in typeSymbol.GetTypeMembers())
            {
                // POLY_ENUM_STRUCT_0003 — generic methods on IEnumCase interface
                if (nested.TypeKind == TypeKind.Interface
                    && string.Equals(nested.Name, INTERFACE_NAME, StringComparison.Ordinal)
                )
                {
                    foreach (var member in nested.GetMembers())
                    {
                        if (member is IMethodSymbol method
                            && method.MethodKind == MethodKind.Ordinary
                            && method.TypeParameters.Length > 0
                        )
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                  IEnumCaseMethodMustNotBeGeneric
                                , method.Locations.Length > 0 ? method.Locations[0] : location
                                , method.Name
                            ));
                        }
                    }

                    continue;
                }

                // Only process non-generic nested structs as case structs
                if (nested.TypeKind != TypeKind.Struct || nested.TypeParameters.Length > 0)
                {
                    continue;
                }

                // Skip the implicitly understood undefined case struct names
                if (string.Equals(nested.Name, UNDEFINED_NAME, StringComparison.Ordinal)
                    || string.Equals(nested.Name, verboseUndefinedName, StringComparison.Ordinal)
                )
                {
                    continue;
                }

                validCaseCount++;

                // POLY_ENUM_STRUCT_0004 — generic methods on case structs
                foreach (var member in nested.GetMembers())
                {
                    if (member is IMethodSymbol method
                        && method.MethodKind == MethodKind.Ordinary
                        && method.TypeParameters.Length > 0
                        && method.ExplicitInterfaceImplementations.Length == 0
                    )
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              CaseStructMethodMustNotBeGeneric
                            , method.Locations.Length > 0 ? method.Locations[0] : location
                            , method.Name
                            , nested.Name
                        ));
                    }
                }
            }

            // POLY_ENUM_STRUCT_0002 — no valid case structs declared
            if (validCaseCount == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustHaveCaseStructs
                    , location
                    , parentName
                ));
            }
        }
    }
}
