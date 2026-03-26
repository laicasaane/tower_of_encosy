using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Entities.Stats;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Generators.Entities.Stats
{
    /// <summary>
    /// Analyzer that reports validation diagnostics for structs annotated with
    /// <c>[StatCollection]</c>, covering conditions that would cause the source generator
    /// to silently produce no output.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StatCollectionDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = StatGeneratorAPI.NAMESPACE;
        private const string STAT_COLLECTION_ATTRIBUTE = $"global::{NAMESPACE}.StatCollectionAttribute";
        private const string STAT_SYSTEM_ATTRIBUTE = $"global::{NAMESPACE}.StatSystemAttribute";
        private const string STAT_DATA_ATTRIBUTE = $"global::{NAMESPACE}.StatDataAttribute";

        public static readonly DiagnosticDescriptor StatSystemAttributeRequired = new(
              id: "SG_STAT_COLLECTION_0001"
            , title: "Type argument of [StatCollection] must have [StatSystem]"
            , messageFormat: "\"{0}\" does not have the [StatSystem] attribute. The typeof argument of [StatCollection] must resolve to a type attributed with [StatSystem]."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The type passed to [StatCollection(typeof(...))] must be attributed with [StatSystem]."
        );

        public static readonly DiagnosticDescriptor TypeIdOffsetOverflow = new(
              id: "SG_STAT_COLLECTION_0002"
            , title: "typeIdOffset + StatData count exceeds uint.MaxValue"
            , messageFormat: "The combination of typeIdOffset ({0}) and the number of [StatData] members ({1}) in \"{2}\" exceeds uint.MaxValue. Reduce typeIdOffset or the number of [StatData] members."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The combination of typeIdOffset and the number of [StatData] nested structs must not exceed uint.MaxValue."
        );

        public static readonly DiagnosticDescriptor MustBeStruct = new(
              id: "SG_STAT_COLLECTION_0003"
            , title: "[StatCollection] can only be applied to a struct"
            , messageFormat: "\"{0}\" is not a struct. [StatCollection] can only be applied to struct types."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatCollection] can only be applied to struct types."
        );

        public static readonly DiagnosticDescriptor MustNotBeNonGeneric = new(
              id: "SG_STAT_COLLECTION_0004"
            , title: "[StatCollection] cannot be applied to an non-generic struct"
            , messageFormat: "\"{0}\" is a generic type. [StatCollection] can only be applied to non-generic structs."
            , category: "StatCollectionGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[StatCollection] can only be applied to non-generic struct types."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustBeStruct
                , MustNotBeNonGeneric
                , StatSystemAttributeRequired
                , TypeIdOffsetOverflow
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
                || typeSymbol.HasAttribute(STAT_COLLECTION_ATTRIBUTE) == false
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
                      MustNotBeNonGeneric
                    , location
                    , typeSymbol.Name
                ));

                return;
            }

            var attrib = typeSymbol.GetAttribute(STAT_COLLECTION_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
            {
                return;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type)
            {
                return;
            }


            if (typeArg.Value is not INamedTypeSymbol statSystemTypeSymbol
                || statSystemTypeSymbol.HasAttribute(STAT_SYSTEM_ATTRIBUTE) == false
            )
            {
                var displayName = (typeArg.Value as ISymbol)?.ToDisplayString() ?? typeArg.Value?.ToString() ?? "?";
                var location = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation()
                    ?? typeSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(
                      StatSystemAttributeRequired
                    , location
                    , displayName
                ));

                return;
            }

            var statDataCount = 0;

            foreach (var member in typeSymbol.GetTypeMembers())
            {
                if (member.TypeKind == TypeKind.Struct
                    && member.IsUnboundGenericType == false
                    && member.HasAttribute(STAT_DATA_ATTRIBUTE))
                {
                    statDataCount++;
                }
            }

            var typeIdOffset = 0uL;

            if (attrib.ConstructorArguments.Length > 1
                && attrib.ConstructorArguments[1].Value is uint offset)
            {
                typeIdOffset = offset;
            }

            if (typeIdOffset + (ulong)statDataCount > uint.MaxValue)
            {
                var location = typeSymbol.Locations.Length > 0
                    ? typeSymbol.Locations[0]
                    : Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                      TypeIdOffsetOverflow
                    , location
                    , typeIdOffset
                    , statDataCount
                    , typeSymbol.Name
                ));
            }
        }
    }
}
