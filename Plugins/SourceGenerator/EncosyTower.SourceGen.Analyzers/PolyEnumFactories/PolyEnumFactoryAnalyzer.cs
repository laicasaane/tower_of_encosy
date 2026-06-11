using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.PolyEnumFactories
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class PolyEnumFactoryAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string POLY_ENUM_FACTORY_FOR_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumFactoryForAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string CATEGORY = "PolyEnumFactoryGenerator";

        public static readonly DiagnosticDescriptor MustBePartial = new(
              id: "SG_POLY_ENUM_FACTORY_0001"
            , title: "[PolyEnumFactoryFor] target must be partial"
            , messageFormat: "\"{0}\" is decorated with [PolyEnumFactoryFor] but is not declared as partial. Add the partial keyword to allow code generation."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Types decorated with [PolyEnumFactoryFor] must be partial so the generator can extend them."
        );

        public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
              id: "SG_POLY_ENUM_FACTORY_0002"
            , title: "[PolyEnumFactoryFor] target must not be generic"
            , messageFormat: "\"{0}\" is generic. [PolyEnumFactoryFor] is not supported on generic types."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Types decorated with [PolyEnumFactoryFor] must not declare type parameters."
        );

        public static readonly DiagnosticDescriptor TargetMustBePolyEnumStruct = new(
              id: "SG_POLY_ENUM_FACTORY_0003"
            , title: "[PolyEnumFactoryFor] target type must be a [PolyEnumStruct]"
            , messageFormat: "Type \"{0}\" passed to [PolyEnumFactoryFor] is not decorated with [PolyEnumStruct]. Factory generation requires a poly-enum struct."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[PolyEnumFactoryFor(typeof(T))] requires T to be decorated with [PolyEnumStruct]."
        );

        public static readonly DiagnosticDescriptor TargetMustNotBeGeneric = new(
              id: "SG_POLY_ENUM_FACTORY_0004"
            , title: "[PolyEnumFactoryFor] target type must not be generic"
            , messageFormat: "Type \"{0}\" passed to [PolyEnumFactoryFor] is generic. Factory generation does not support generic poly-enum structs."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type argument passed to [PolyEnumFactoryFor] must not declare type parameters."
        );

        public static readonly DiagnosticDescriptor MustHaveCaseStructs = new(
              id: "SG_POLY_ENUM_FACTORY_0005"
            , title: "[PolyEnumFactoryFor] target type has no case structs"
            , messageFormat: "Type \"{0}\" has no eligible case structs. The generated factory will only contain an Undefined() method."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The poly-enum struct passed to [PolyEnumFactoryFor] should declare at least one nested case struct."
        );

        public static readonly DiagnosticDescriptor CaseCtorOutParameterIgnored = new(
              id: "SG_POLY_ENUM_FACTORY_0006"
            , title: "Case constructor with out parameter is ignored"
            , messageFormat: "Constructor of case struct \"{0}\" has an out parameter and will be skipped by [PolyEnumFactoryFor] code generation."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Factory methods cannot forward out parameters. Such constructors are ignored when generating factories."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustBePartial
                , MustNotBeGeneric
                , TargetMustBePolyEnumStruct
                , TargetMustNotBeGeneric
                , MustHaveCaseStructs
                , CaseCtorOutParameterIgnored
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(POLY_ENUM_FACTORY_FOR_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(POLY_ENUM_FACTORY_FOR_ATTRIBUTE, token);
            var attribLocation = attrib?.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? typeSymbol.Locations[0];
            var typeLocation = typeSymbol.Locations.Length > 0 ? typeSymbol.Locations[0] : attribLocation;

            if (IsDeclaredPartial(typeSymbol, token) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustBePartial
                    , typeLocation
                    , typeSymbol.Name
                ));
            }

            if (typeSymbol.TypeParameters.Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustNotBeGeneric
                    , typeLocation
                    , typeSymbol.Name
                ));
            }

            if (attrib == null
                || attrib.ConstructorArguments.Length < 1
                || attrib.ConstructorArguments[0].Value is not INamedTypeSymbol enumStructSymbol
            )
            {
                return;
            }

            if (enumStructSymbol.IsGenericType || enumStructSymbol.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TargetMustNotBeGeneric
                    , attribLocation
                    , enumStructSymbol.Name
                ));
                return;
            }

            if (enumStructSymbol.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TargetMustBePolyEnumStruct
                    , attribLocation
                    , enumStructSymbol.Name
                ));
                return;
            }

            token.ThrowIfCancellationRequested();

            var hasCase = false;

            foreach (var nested in enumStructSymbol.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                if (nested.TypeKind != TypeKind.Struct)
                {
                    continue;
                }

                if (nested.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
                {
                    continue;
                }

                hasCase = true;
                ReportOutParamCtors(context, nested);
            }

            if (hasCase == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustHaveCaseStructs
                    , attribLocation
                    , enumStructSymbol.Name
                ));
            }
        }

        private static bool IsDeclaredPartial(INamedTypeSymbol typeSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (syntaxRef.GetSyntax(token) is TypeDeclarationSyntax typeSyntax)
                {
                    foreach (var modifier in typeSyntax.Modifiers)
                    {
                        token.ThrowIfCancellationRequested();

                        if (modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void ReportOutParamCtors(SymbolAnalysisContext context, INamedTypeSymbol caseSymbol)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var ctor in caseSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (ctor.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (ctor.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var hasOut = false;

                foreach (var p in ctor.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    if (p.RefKind == RefKind.Out)
                    {
                        hasOut = true;
                        break;
                    }
                }

                if (hasOut == false)
                {
                    continue;
                }

                var location = ctor.Locations.Length > 0 ? ctor.Locations[0] : caseSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      CaseCtorOutParameterIgnored
                    , location
                    , caseSymbol.Name
                ));
            }
        }
    }
}
