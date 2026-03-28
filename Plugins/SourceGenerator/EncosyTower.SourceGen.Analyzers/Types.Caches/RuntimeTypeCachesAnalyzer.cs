using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.Types.Caches;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Analyzers.Types.Caches
{
    /// <summary>
    /// Analyzer that reports all validation diagnostics for <c>RuntimeTypeCache.GetXxx&lt;T&gt;()</c>
    /// call sites. Keeping diagnostics in a <see cref="DiagnosticAnalyzer"/> rather than inside the
    /// source generator itself prevents unnecessary regeneration of source files when only an error
    /// (and not the surrounding valid code) has changed.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class RuntimeTypeCachesAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.Types.Caches";
        private const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        private const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Types.RuntimeTypeCache";

        public static readonly DiagnosticDescriptor TypeParameterIsNotApplicable = new(
              id: "RUNTIME_TYPE_CACHES_0001"
            , title: "Type parameter is not applicable"
            , messageFormat: "\"{0}\" is a type parameter thus it is not applicable for the \"{1}\" method"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Type parameter is not applicable."
        );

        public static readonly DiagnosticDescriptor OnlyClassOrInterfaceIsApplicable = new(
              id: "RUNTIME_TYPE_CACHES_0002"
            , title: "Only class or interface is applicable"
            , messageFormat: "\"{0}\" is not applicable because it is not class nor interface"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Only class or interface is applicable."
        );

        public static readonly DiagnosticDescriptor StaticClassIsNotApplicable = new(
              id: "RUNTIME_TYPE_CACHES_0003"
            , title: "Static class is not applicable"
            , messageFormat: "\"{0}\" is not applicable because it is static"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Static class is not applicable."
        );

        public static readonly DiagnosticDescriptor SealedClassIsNotApplicable = new(
              id: "RUNTIME_TYPE_CACHES_0004"
            , title: "Sealed class is not applicable"
            , messageFormat: "\"{0}\" is not applicable because it is sealed"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Sealed class is not applicable."
        );

        public static readonly DiagnosticDescriptor AssemblyNameMustBeStringLiteralOrConstant = new(
              id: "RUNTIME_TYPE_CACHES_0005"
            , title: "Assembly name must be a string literal or constant"
            , messageFormat: "Assembly name must be a string literal or constant"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Assembly name must be a string literal or constant."
        );

        public static readonly DiagnosticDescriptor TypesFromCachesAreProhibited = new(
              id: "RUNTIME_TYPE_CACHES_0006"
            , title: "Types from \"EncosyTower.Types.Caches\" are prohibited"
            , messageFormat: "\"{0}\" is prohibited"
            , category: "RuntimeTypeCachesGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Types from \"EncosyTower.Types.Caches\" are prohibited."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  TypeParameterIsNotApplicable
                , OnlyClassOrInterfaceIsApplicable
                , StaticClassIsNotApplicable
                , SealedClassIsNotApplicable
                , AssemblyNameMustBeStringLiteralOrConstant
                , TypesFromCachesAreProhibited
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                return;
            }

            // Quick syntax filter: must look like RuntimeTypeCache.GetXxx<T>
            if (memberAccess.Expression is not IdentifierNameSyntax { Identifier.ValueText: "RuntimeTypeCache" })
            {
                return;
            }

            if (memberAccess.Name is not GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } genericName)
            {
                return;
            }

            var methodName = genericName.Identifier.ValueText;

            if (methodName is not (
                    RuntimeTypeCacheMethods.GET_INFO
                 or RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM
                 or RuntimeTypeCacheMethods.GET_TYPES_WITH_ATTRIBUTE
                 or RuntimeTypeCacheMethods.GET_FIELDS_WITH_ATTRIBUTE
                 or RuntimeTypeCacheMethods.GET_METHODS_WITH_ATTRIBUTE))
            {
                return;
            }

            // Semantic verification: the LHS must actually be EncosyTower.Types.RuntimeTypeCache.
            var semanticModel = context.SemanticModel;
            var identifierType = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;

            if (identifierType is null || !identifierType.HasFullName(RUNTIME_TYPE_CACHE))
            {
                return;
            }

            // ── Validate the type argument ──────────────────────────────────────────────
            var typeArgSyntax = genericName.TypeArgumentList.Arguments[0];
            var typeInfo = semanticModel.GetTypeInfo(typeArgSyntax, context.CancellationToken);
            var type = typeInfo.Type;

            if (type is null)
            {
                return;
            }

            if (type is ITypeParameterSymbol)
            {
                var paramName = (typeArgSyntax as IdentifierNameSyntax)?.Identifier.ValueText ?? "T";
                context.ReportDiagnostic(Diagnostic.Create(
                      TypeParameterIsNotApplicable
                    , typeArgSyntax.GetLocation()
                    , paramName
                    , methodName));
                return;
            }

            if (type is not INamedTypeSymbol namedType)
            {
                return;
            }

            var typeFullName = namedType.ToFullName();

            if (methodName == RuntimeTypeCacheMethods.GET_INFO)
            {
                if (namedType.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          StaticClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                }
            }
            else if (methodName == RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM)
            {
                if (namedType.TypeKind is not (TypeKind.Class or TypeKind.Interface))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          OnlyClassOrInterfaceIsApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                    return;
                }

                if (namedType.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          StaticClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                    return;
                }

                if (namedType.IsSealed)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          SealedClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                }
            }
            else
            {
                // GetTypesWithAttribute, GetFieldsWithAttribute, GetMethodsWithAttribute
                if (typeFullName.StartsWith(NAMESPACE_PREFIX))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          TypesFromCachesAreProhibited
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                }
            }

            // ── Validate the optional assembly-name argument ────────────────────────────
            if (invocation.ArgumentList.Arguments.Count >= 1)
            {
                var argExpr = invocation.ArgumentList.Arguments[0].Expression;
                var constValue = semanticModel.GetConstantValue(argExpr, context.CancellationToken);

                if (constValue is not { HasValue: true, Value: string })
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          AssemblyNameMustBeStringLiteralOrConstant
                        , argExpr.GetLocation()));
                }
            }
        }
    }
}
