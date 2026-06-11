using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.SourceGen.Helpers.UnionIds;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.UnionIds
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class UnionIdAnalyzer : DiagnosticAnalyzer
    {
        private const string UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdAttribute";
        private const string KIND_FOR_UNION_ID_ATTRIBUTE = "global::EncosyTower.UnionIds.KindForUnionIdAttribute";
        private const string UNION_ID_KIND_ATTRIBUTE = "global::EncosyTower.UnionIds.UnionIdKindAttribute";

        public static readonly DiagnosticDescriptor SameKindIsIgnored = new(
              id: "SG_UNION_ID_0001"
            , title: "A kind of the same name will be ignored"
            , messageFormat: "Kind \"{0}\" has already been defined by another type \"{1}\""
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A kind of the same name will be ignored"
        );

        public static readonly DiagnosticDescriptor MustBeUnmanagedType = new(
              id: "SG_UNION_ID_0002"
            , title: "First parameter must be an unmanaged type"
            , messageFormat: "First parameter must be an unmanaged type"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "First parameter must be an unmanaged type"
        );

        public static readonly DiagnosticDescriptor KindTypeCannotBeIdType = new(
              id: "SG_UNION_ID_0003"
            , title: "Cannot use the id type as a kind of itself"
            , messageFormat: "Cannot use \"{0}\" as a kind of itself"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Cannot use the id type as a kind of itself"
        );

        public static readonly DiagnosticDescriptor TypeAlreadyDeclared = new(
              id: "SG_UNION_ID_0004"
            , title: "Type has already been declared"
            , messageFormat: "Type \"{0}\" has already been declared"
            , category: "UnionIdGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type has already been declared"
        );

        public static readonly DiagnosticDescriptor KindSizeMustBeSmallerThan16Bytes = new(
              id: "SG_UNION_ID_0005"
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

            context.RegisterSymbolAction(AnalyzeIdSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeIdSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            if (typeSymbol.TypeKind == TypeKind.Struct
                && typeSymbol.HasAttribute(UNION_ID_ATTRIBUTE, token)
            )
            {
                if (typeSymbol.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          MustBeUnmanagedType
                        , typeSymbol.Locations[0]
                    , token));
                    return;
                }

                ValidateInlineKindAttributes(context, typeSymbol);
                return;
            }

            if (typeSymbol.HasAttribute(KIND_FOR_UNION_ID_ATTRIBUTE, token) == false)
                return;

            var attrib = typeSymbol.GetAttribute(KIND_FOR_UNION_ID_ATTRIBUTE, token);

            if (attrib == null || attrib.ConstructorArguments.Length < 1)
                return;

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type)
                return;

            if (typeArg.Value is not INamedTypeSymbol idSymbol)
                return;

            if (typeSymbol.IsUnmanagedType == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      MustBeUnmanagedType
                    , attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0]
                , token));
                return;
            }

            if (SymbolEqualityComparer.Default.Equals(typeSymbol, idSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      KindTypeCannotBeIdType
                    , attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0]
                    , typeSymbol.ToDisplayString()
                , token));
            }

            var size = 0;
            typeSymbol.GetUnmanagedSize(ref size, token);

            if (size >= (int)UnionIdSize.ULong2)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      KindSizeMustBeSmallerThan16Bytes
                    , attrib.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation() ?? typeSymbol.Locations[0]
                    , typeSymbol.ToDisplayString()
                , token));
            }
        }

        private static void ValidateInlineKindAttributes(
              SymbolAnalysisContext context
            , INamedTypeSymbol idSymbol
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            var seenNames = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var seenKindNames = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);

            foreach (var attrib in idSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attrib.AttributeClass?.HasFullName(UNION_ID_KIND_ATTRIBUTE, token) != true)
                    continue;

                if (attrib.ConstructorArguments.Length < 1)
                    continue;

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind != TypedConstantKind.Type)
                    continue;

                if (typeArg.Value is not INamedTypeSymbol kindSymbol)
                    continue;

                var loc = attrib.ApplicationSyntaxReference?.GetSyntax(token)?
                    .GetLocation() ?? idSymbol.Locations[0];

                if (kindSymbol.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MustBeUnmanagedType, loc, token));
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(kindSymbol, idSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(KindTypeCannotBeIdType, loc, idSymbol.ToDisplayString(), token));
                    continue;
                }

                if (seenNames.Contains(kindSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(TypeAlreadyDeclared, loc, kindSymbol.ToDisplayString(), token));
                    continue;
                }

                seenNames.Add(kindSymbol);

                string customName = null;

                if (attrib.ConstructorArguments.Length >= 3
                    && attrib.ConstructorArguments[2].Value is string nameVal
                )
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
                kindSymbol.GetUnmanagedSize(ref size, token);

                if (size >= (int)UnionIdSize.ULong2)
                {
                    context.ReportDiagnostic(Diagnostic.Create(KindSizeMustBeSmallerThan16Bytes, loc, kindName, token));
                }
            }
        }
    }
}
