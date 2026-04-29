using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.TypeWraps
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class TypeWrapDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.TypeWraps";
        private const string WRAP_TYPE_FQN = $"global::{NAMESPACE}.WrapTypeAttribute";
        private const string WRAP_RECORD_FQN = $"global::{NAMESPACE}.WrapRecordAttribute";
        private const string CATEGORY = "TypeWrapGenerator";

        public static readonly DiagnosticDescriptor WrapTypeOnRecord = new(
              id: "SG_TYPE_WRAP_0001"
            , title: "[WrapType] is not allowed on record"
            , messageFormat: "[WrapType] cannot be applied to record type \"{0}\". Use [WrapRecord] instead."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[WrapType] is not allowed on record."
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new(
              id: "SG_TYPE_WRAP_0002"
            , title: "[WrapType] first argument must be a typeof expression"
            , messageFormat: "[WrapType] on \"{0}\" requires the first argument to be a typeof(...) expression"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[WrapType] first argument must be a typeof expression."
        );

        public static readonly DiagnosticDescriptor InvalidMemberName = new(
              id: "SG_TYPE_WRAP_0003"
            , title: "[WrapType] MemberName must be a valid C# identifier"
            , messageFormat: "[WrapType] on \"{0}\" requires MemberName \"{1}\" to be a non-empty, valid C# identifier"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[WrapType] MemberName must be a valid C# identifier."
        );

        public static readonly DiagnosticDescriptor WrapRecordRequiresOneParameter = new(
              id: "SG_TYPE_WRAP_0004"
            , title: "[WrapRecord] requires a positional record with exactly one parameter"
            , messageFormat: "[WrapRecord] on \"{0}\" requires a positional record declaration with exactly one parameter"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[WrapRecord] requires a positional record with exactly one parameter."
        );

        public static readonly DiagnosticDescriptor WrapperInheritsBaseClass = new(
              id: "SG_TYPE_WRAP_0005"
            , title: "Wrapper class must not inherit a base class"
            , messageFormat: "Wrapper class \"{0}\" must not inherit a base class other than System.Object"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Wrapper class must not inherit a base class."
        );

        public static readonly DiagnosticDescriptor WrapRecordOnNonRecord = new(
              id: "SG_TYPE_WRAP_0006"
            , title: "[WrapRecord] is not allowed on non-record declaration"
            , messageFormat: "[WrapRecord] cannot be applied to \"{0}\" because it is not a record declaration"
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[WrapRecord] is not allowed on non-record declaration."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  WrapTypeOnRecord
                , NotTypeOfExpression
                , InvalidMemberName
                , WrapRecordRequiresOneParameter
                , WrapperInheritsBaseClass
                , WrapRecordOnNonRecord
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not INamedTypeSymbol typeSymbol)
            {
                return;
            }

            var ct = context.CancellationToken;
            var hasWrapType = typeSymbol.TryGetAttribute(WRAP_TYPE_FQN, out var wrapTypeAttr);
            var hasWrapRecord = typeSymbol.TryGetAttribute(WRAP_RECORD_FQN, out var wrapRecordAttr);

            if (hasWrapType == false && hasWrapRecord == false)
            {
                return;
            }

            if (hasWrapType)
            {
                AnalyzeWrapType(context, typeSymbol, wrapTypeAttr, ct);
            }

            if (hasWrapRecord)
            {
                AnalyzeWrapRecord(context, typeSymbol, wrapRecordAttr, ct);
            }

            if (typeSymbol.TypeKind == TypeKind.Class && InheritsNonObjectBaseClass(typeSymbol))
            {
                var attr = wrapTypeAttr ?? wrapRecordAttr;
                var location = GetAttributeLocation(attr, typeSymbol, ct);

                context.ReportDiagnostic(Diagnostic.Create(
                      WrapperInheritsBaseClass
                    , location
                    , typeSymbol.Name
                ));
            }
        }

        private static void AnalyzeWrapType(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , AttributeData attr
            , CancellationToken ct
        )
        {
            var location = GetAttributeLocation(attr, typeSymbol, ct);

            if (typeSymbol.IsRecord)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      WrapTypeOnRecord
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            var args = attr.ConstructorArguments;

            if (args.Length < 1
                || args[0].Kind != TypedConstantKind.Type
                || args[0].Value is not ITypeSymbol
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      NotTypeOfExpression
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            if (args.Length >= 2 && args[1].Kind == TypedConstantKind.Primitive)
            {
                var memberName = args[1].Value as string;

                if (IsValidMemberName(memberName) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidMemberName
                        , location
                        , typeSymbol.Name
                        , memberName ?? string.Empty
                    ));
                }
            }
        }

        private static bool IsValidMemberName(string name)
        {
            if (SyntaxFacts.IsValidIdentifier(name) == false)
            {
                return false;
            }

            if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
            {
                return false;
            }

            return true;
        }

        private static void AnalyzeWrapRecord(
              SymbolAnalysisContext context
            , INamedTypeSymbol typeSymbol
            , AttributeData attr
            , CancellationToken ct
        )
        {
            var location = GetAttributeLocation(attr, typeSymbol, ct);

            if (typeSymbol.IsRecord == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      WrapRecordOnNonRecord
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            if (TryGetRecordParameterCount(typeSymbol, ct, out var paramCount) && paramCount == 1)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  WrapRecordRequiresOneParameter
                , location
                , typeSymbol.Name
            ));
        }

        private static bool TryGetRecordParameterCount(
              INamedTypeSymbol typeSymbol
            , CancellationToken ct
            , out int parameterCount
        )
        {
            parameterCount = 0;

            foreach (var reference in typeSymbol.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(ct) is RecordDeclarationSyntax recordSyntax
                    && recordSyntax.ParameterList is ParameterListSyntax parameterList
                )
                {
                    parameterCount = parameterList.Parameters.Count;
                    return true;
                }
            }

            return false;
        }

        private static bool InheritsNonObjectBaseClass(INamedTypeSymbol typeSymbol)
        {
            var baseType = typeSymbol.BaseType;

            while (baseType != null)
            {
                if (baseType.TypeKind == TypeKind.Class
                    && baseType.SpecialType != SpecialType.System_Object
                )
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private static Location GetAttributeLocation(
              AttributeData attr
            , INamedTypeSymbol typeSymbol
            , CancellationToken ct
        )
        {
            if (attr is { ApplicationSyntaxReference: { } reference })
            {
                var syntax = reference.GetSyntax(ct);

                if (syntax != null)
                {
                    return syntax.GetLocation();
                }
            }

            return typeSymbol.Locations.Length > 0
                ? typeSymbol.Locations[0]
                : Location.None;
        }
    }
}
