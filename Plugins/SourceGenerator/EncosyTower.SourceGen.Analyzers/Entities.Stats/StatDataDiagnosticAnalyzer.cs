using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for types annotated with
    /// <c>[StatData]</c>, covering conditions that would cause the source generator
    /// to silently produce no output or invalid output.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StatDataDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string STAT_DATA_ATTRIBUTE = $"global::{NAMESPACE}.StatDataAttribute";

        public static readonly DiagnosticDescriptor MustBeStruct = new(
              id: "SG_STAT_DATA_0001"
            , title: "[StatData] can only be applied to a struct"
            , messageFormat: "\"{0}\" is not a struct. [StatData] can only be applied to struct types."
            , category: "StatDataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatData] can only be applied to struct types."
        );

        public static readonly DiagnosticDescriptor MustNotBeGeneric = new(
              id: "SG_STAT_DATA_0002"
            , title: "[StatData] cannot be applied to a generic struct"
            , messageFormat: "\"{0}\" is a generic type. [StatData] can only be applied to non-generic structs."
            , category: "StatDataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatData] can only be applied to non-generic struct types."
        );

        public static readonly DiagnosticDescriptor StatVariantTypeMustNotBeNone = new(
              id: "SG_STAT_DATA_0003"
            , title: "StatVariantType.None is not a valid argument for [StatData]"
            , messageFormat: "The [StatData] attribute on \"{0}\" uses StatVariantType.None, which is not a valid variant type and will produce no output."
            , category: "StatDataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "StatVariantType.None is reserved and cannot be used as the type argument for [StatData]. Use a concrete StatVariantType value instead."
        );

        public static readonly DiagnosticDescriptor TypeofArgMustBeEnum = new(
              id: "SG_STAT_DATA_0004"
            , title: "typeof argument of [StatData] must be an enum type"
            , messageFormat: "\"{0}\" is not an enum. The typeof argument of [StatData] must resolve to an enum type."
            , category: "StatDataGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [StatData(typeof(...))] must be an enum type."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustBeStruct
                , MustNotBeGeneric
                , StatVariantTypeMustNotBeNone
                , TypeofArgMustBeEnum
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(STAT_DATA_ATTRIBUTE) == false
            )
            {
                return;
            }

            if (typeSymbol.TypeKind != TypeKind.Struct)
            {
                var location = typeSymbol.Locations.Length > 0
                    ? typeSymbol.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      MustBeStruct
                    , location
                    , typeSymbol.Name
                ));

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

                return;
            }

            var attrib = typeSymbol.GetAttribute(STAT_DATA_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
            {
                return;
            }

            var arg = attrib.ConstructorArguments[0];

            if (arg.Kind == TypedConstantKind.Enum && arg.Value is byte enumValue && enumValue == 0)
            {
                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      StatVariantTypeMustNotBeNone
                    , location
                    , typeSymbol.Name
                ));

                return;
            }

            if (arg.Kind == TypedConstantKind.Type
                && (arg.Value is not INamedTypeSymbol typeArg
                    || typeArg.TypeKind != TypeKind.Enum)
            )
            {
                var displayName = (arg.Value as ISymbol)?.ToDisplayString() ?? arg.Value?.ToString() ?? "?";
                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      TypeofArgMustBeEnum
                    , location
                    , displayName
                ));
            }
        }
    }
}
