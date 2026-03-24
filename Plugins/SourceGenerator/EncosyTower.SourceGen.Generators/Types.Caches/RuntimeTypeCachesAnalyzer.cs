using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008 // Enable analyzer release tracking

namespace EncosyTower.SourceGen.Generators.Types.Caches
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  DiagnosticDescriptors.TypeParameterIsNotApplicable
                , DiagnosticDescriptors.OnlyClassOrInterfaceIsApplicable
                , DiagnosticDescriptors.StaticClassIsNotApplicable
                , DiagnosticDescriptors.SealedClassIsNotApplicable
                , DiagnosticDescriptors.AssemblyNameMustBeStringLiteralOrConstant
                , DiagnosticDescriptors.TypesFromCachesAreProhibited
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
                    RuntimeTypeCachesGenerator.METHOD_GET_INFO
                 or RuntimeTypeCachesGenerator.METHOD_GET_TYPES_DERIVED_FROM
                 or RuntimeTypeCachesGenerator.METHOD_GET_TYPES_WITH_ATTRIBUTE
                 or RuntimeTypeCachesGenerator.METHOD_GET_FIELDS_WITH_ATTRIBUTE
                 or RuntimeTypeCachesGenerator.METHOD_GET_METHODS_WITH_ATTRIBUTE))
            {
                return;
            }

            // Semantic verification: the LHS must actually be EncosyTower.Types.RuntimeTypeCache.
            var semanticModel = context.SemanticModel;
            var identifierType = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;

            if (identifierType?.ToFullName() is not RuntimeTypeCachesGenerator.RUNTIME_TYPE_CACHE)
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
                      DiagnosticDescriptors.TypeParameterIsNotApplicable
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

            if (methodName == RuntimeTypeCachesGenerator.METHOD_GET_INFO)
            {
                if (namedType.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.StaticClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                }
            }
            else if (methodName == RuntimeTypeCachesGenerator.METHOD_GET_TYPES_DERIVED_FROM)
            {
                if (namedType.TypeKind is not (TypeKind.Class or TypeKind.Interface))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.OnlyClassOrInterfaceIsApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                    return;
                }

                if (namedType.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.StaticClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                    return;
                }

                if (namedType.IsSealed)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.SealedClassIsNotApplicable
                        , typeArgSyntax.GetLocation()
                        , typeFullName));
                }
            }
            else
            {
                // GetTypesWithAttribute, GetFieldsWithAttribute, GetMethodsWithAttribute
                if (typeFullName.StartsWith(RuntimeTypeCachesGenerator.NAMESPACE_PREFIX))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          DiagnosticDescriptors.TypesFromCachesAreProhibited
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
                          DiagnosticDescriptors.AssemblyNameMustBeStringLiteralOrConstant
                        , argExpr.GetLocation()));
                }
            }
        }
    }
}
