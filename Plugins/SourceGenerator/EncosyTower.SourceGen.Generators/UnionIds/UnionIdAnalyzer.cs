using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    /// <summary>
    /// Analyzer that reports all validation diagnostics for types annotated with
    /// <c>[UnionId]</c> and <c>[KindForUnionId]</c> / <c>[UnionIdKind]</c>.
    /// Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator itself prevents unnecessary regeneration of source files when only
    /// an error (and not the surrounding valid code) has changed.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UnionIdAnalyzer : DiagnosticAnalyzer
    {
        private const string UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdAttribute";
        private const string KIND_FOR_UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.KindForUnionIdAttribute";
        private const string UNION_ID_KIND_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdKindAttribute";

        public static readonly DiagnosticDescriptor SameKindIsIgnored = new(
              id: "UNION_ID_0001"
            , title: "A kind of the same name will be ignored"
            , messageFormat: "Kind \"{0}\" has already been defined by another type \"{1}\""
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A kind of the same name will be ignored"
        );

        public static readonly DiagnosticDescriptor MustBeUnmanagedType = new(
              id: "UNION_ID_0002"
            , title: "First parameter must be an unmanaged type"
            , messageFormat: "First parameter must be an unmanaged type"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "First parameter must be an unmanaged type"
        );

        public static readonly DiagnosticDescriptor KindTypeCannotBeIdType = new(
              id: "UNION_ID_0003"
            , title: "Cannot use the id type as a kind of itself"
            , messageFormat: "Cannot use \"{0}\" as a kind of itself"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Cannot use the id type as a kind of itself"
        );

        public static readonly DiagnosticDescriptor TypeAlreadyDeclared = new(
              id: "UNION_ID_0004"
            , title: "Type has already been declared"
            , messageFormat: "Type \"{0}\" has already been declared"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type has already been declared"
        );

        public static readonly DiagnosticDescriptor KindSizeMustBeSmallerThan16Bytes = new(
              id: "UNION_ID_0005"
            , title: "The size of a type is equal to or larger than 16 bytes"
            , messageFormat: "The size of \"{0}\" is equal to or larger than 16 bytes"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The size of a type must be smaller than 16 bytes"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  SameKindIsIgnored
                , MustBeUnmanagedType
                , KindTypeCannotBeIdType
                , TypeAlreadyDeclared
                , KindSizeMustBeSmallerThan16Bytes
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Per-symbol checks: MustBeUnmanagedType (0002), KindTypeCannotBeIdType (0003)
            context.RegisterSymbolAction(AnalyzeIdSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeIdSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
                return;

            // --- Validate [UnionId] structs ---
            if (typeSymbol.TypeKind == TypeKind.Struct
                && typeSymbol.HasAttribute(UNION_ID_ATTRIBUTE))
            {
                if (typeSymbol.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MustBeUnmanagedType
                        , typeSymbol.Locations[0]
                    ));
                    return;
                }

                // Check [UnionIdKind] inline attributes for per-kind validation
                ValidateInlineKindAttributes(context, typeSymbol);
                return;
            }

            // --- Validate [KindForUnionId] types ---
            if (typeSymbol.HasAttribute(KIND_FOR_UNION_ID_ATTRIBUTE) == false)
                return;

            var attrib = typeSymbol.GetAttribute(KIND_FOR_UNION_ID_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
                return;

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type)
                return;

            if (typeArg.Value is not INamedTypeSymbol idSymbol)
                return;

            // Inline-kind must be unmanaged
            if (typeSymbol.IsUnmanagedType == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustBeUnmanagedType
                    , attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? typeSymbol.Locations[0]
                ));
                return;
            }

            // Kind cannot be the same type as the id
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, idSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      KindTypeCannotBeIdType
                    , attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? typeSymbol.Locations[0]
                    , typeSymbol.ToDisplayString()
                ));
            }

            // Validate unmanaged size
            var size = 0;
            typeSymbol.GetUnmanagedSize(ref size);

            if (size >= (int)UnionIdSize.ULong2)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      KindSizeMustBeSmallerThan16Bytes
                    , attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? typeSymbol.Locations[0]
                    , typeSymbol.ToDisplayString()
                ));
            }
        }

        private static void ValidateInlineKindAttributes(SymbolAnalysisContext context, INamedTypeSymbol idSymbol)
        {
            var seenNames = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var seenKindNames = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

            foreach (var attrib in idSymbol.GetAttributes())
            {
                if (attrib.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != UNION_ID_KIND_ATTRIBUTE)
                    continue;

                if (attrib.ConstructorArguments.Length < 1)
                    continue;

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind != TypedConstantKind.Type)
                    continue;

                if (typeArg.Value is not INamedTypeSymbol kindSymbol)
                    continue;

                var loc = attrib.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? idSymbol.Locations[0];

                if (kindSymbol.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MustBeUnmanagedType, loc));
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(kindSymbol, idSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(KindTypeCannotBeIdType, loc, idSymbol.ToDisplayString()));
                    continue;
                }

                if (seenNames.Contains(kindSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(TypeAlreadyDeclared, loc, kindSymbol.ToDisplayString()));
                    continue;
                }

                seenNames.Add(kindSymbol);

                // Custom name de-dup
                string customName = null;

                if (attrib.ConstructorArguments.Length >= 3
                    && attrib.ConstructorArguments[2].Value is string nameVal)
                {
                    customName = nameVal.ToValidIdentifier();
                }

                var kindName = string.IsNullOrEmpty(customName) ? kindSymbol.Name : customName;

                if (seenKindNames.TryGetValue(kindName, out var otherKind))
                {
                    context.ReportDiagnostic(Diagnostic.Create(SameKindIsIgnored, loc, kindName, otherKind.ToDisplayString()));
                    continue;
                }

                seenKindNames.Add(kindName, kindSymbol);

                var size = 0;
                kindSymbol.GetUnmanagedSize(ref size);

                if (size >= (int)UnionIdSize.ULong2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(KindSizeMustBeSmallerThan16Bytes, loc, kindName));
                }
            }
        }
    }
}
