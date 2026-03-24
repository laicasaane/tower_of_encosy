using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalVariants
{
    using InternalVariantDeclaration = Variants.InternalVariantDeclaration;

    [Generator]
    public class InternalVariantGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = MvvmGeneratorHelpers.NAMESPACE;
        public const string SKIP_ATTRIBUTE = MvvmGeneratorHelpers.SKIP_ATTRIBUTE;

        private const string COMPONENT_MODEL_NS = $"global::{NAMESPACE}.ComponentModel";
        private const string VIEW_BINDING_NS = $"{NAMESPACE}.ViewBinding";
        private const string INPUT_NS = $"{NAMESPACE}.Input";
        private const string IOBSERVABLE_OBJECT = $"global::{NAMESPACE}.ComponentModel.IObservableObject";
        private const string IVARIANT_T = "global::EncosyTower.Variants.IVariant<";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatch
                , transform: GetSemanticMatch
            ).Where(static x => x.IsValid);

            var typeNamesToIgnoreProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsStructSyntaxMatch
                , transform: GetTypeNameInGenericVariantDeclaration
            ).Where(static x => string.IsNullOrEmpty(x) == false);

            var combined = candidateProvider.Collect()
                .Combine(typeNamesToIgnoreProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                if (source.Left.Right.isValid == false)
                {
                    return;
                }

                GenerateOutput(
                      sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left.Left
                    , source.Left.Left.Right
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static bool IsStructSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is StructDeclarationSyntax structSyntax
                && structSyntax.BaseList != null
                && structSyntax.BaseList.Types.Count > 0
                && structSyntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameGenericCandidate("EncosyTower.Variants", "IVariant", out _)
                );
        }

        public static bool IsSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is FieldDeclarationSyntax field)
            {
                if (field.HasAttributeCandidate(COMPONENT_MODEL_NS, "ObservableProperty"))
                {
                    return true;
                }
            }

            if (syntaxNode is MethodDeclarationSyntax method && method.ParameterList.Parameters.Count == 1)
            {
                if (method.HasAttributeCandidate(INPUT_NS, "RelayCommand")
                    || method.HasAttributeCandidate(VIEW_BINDING_NS, "BindingProperty")
                    || method.HasAttributeCandidate(VIEW_BINDING_NS, "BindingCommand")
                )
                {
                    return true;
                }
            }

            if (syntaxNode is PropertyDeclarationSyntax property
                && property.Parent is ClassDeclarationSyntax classSyntax
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0
            )
            {
                return true;
            }

            return false;
        }

        internal static InternalVariantDeclaration GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            // Note: IsValidCompilation check deliberately omitted here.
            // Checking compilation validity inside the transform step prevents incremental cache reuse
            // when unrelated files change. Validity is deferred to GenerateOutput via CompilationCandidateSlim.isValid.

            var semanticModel = context.SemanticModel;
            ITypeSymbol typeSymbol;
            LocationInfo location;

            if (context.Node is FieldDeclarationSyntax field)
            {
                typeSymbol = semanticModel.GetTypeInfo(field.Declaration.Type, token).Type;
                location = LocationInfo.From(field.Declaration.Type.GetLocation());
            }
            else if (context.Node is MethodDeclarationSyntax method)
            {
                var typeSyntax = method.ParameterList.Parameters[0].Type;
                typeSymbol = semanticModel.GetTypeInfo(typeSyntax, token).Type;
                location = LocationInfo.From(typeSyntax.GetLocation());
            }
            else if (context.Node is PropertyDeclarationSyntax property
                && property.Parent is ClassDeclarationSyntax classSyntax
                && classSyntax.DoesSemanticMatch(IOBSERVABLE_OBJECT, semanticModel, token)
                && classSyntax.AnyFieldHasNotifyPropertyChangedForAttribute(property)
            )
            {
                typeSymbol = semanticModel.GetTypeInfo(property.Type, token).Type;
                location = LocationInfo.From(property.Type.GetLocation());
            }
            else
            {
                return default;
            }

            if (typeSymbol is not INamedTypeSymbol namedType
                || namedType.IsUnboundGenericType
                || (namedType.IsGenericType && namedType.TypeParameters.Length != 0)
            )
            {
                return default;
            }

            return Generators.Variants.InternalVariantGenerator.BuildDeclaration(namedType, location, token);
        }

        public static string GetTypeNameInGenericVariantDeclaration(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax structSyntax
                || structSyntax.BaseList == null
                || structSyntax.BaseList.Types.Count < 1
            )
            {
                return string.Empty;
            }

            var semanticModel = context.SemanticModel;

            foreach (var baseType in structSyntax.BaseList.Types)
            {
                var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                if (typeInfo.Type == null)
                {
                    continue;
                }

                if (typeInfo.Type is INamedTypeSymbol interfaceSymbol
                    && interfaceSymbol.IsGenericType
                    && interfaceSymbol.TypeParameters.Length == 1
                    && interfaceSymbol.ToFullName().StartsWith(IVARIANT_T)
                )
                {
                    return interfaceSymbol.TypeArguments[0].ToFullName();
                }

                if (TryGetMatchTypeArgument(typeInfo.Type.Interfaces, out var typeName)
                    || TryGetMatchTypeArgument(typeInfo.Type.AllInterfaces, out typeName)
                )
                {
                    return typeName;
                }
            }

            return string.Empty;

            static bool TryGetMatchTypeArgument(
                  ImmutableArray<INamedTypeSymbol> interfaces
                , out string result
            )
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.IsGenericType
                        && interfaceSymbol.TypeParameters.Length == 1
                        && interfaceSymbol.ToFullName().StartsWith(IVARIANT_T)
                    )
                    {
                        result = interfaceSymbol.TypeArguments[0].ToFullName();
                        return true;
                    }
                }

                result = string.Empty;
                return false;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilationCandidate
            , ImmutableArray<InternalVariantDeclaration> candidates
            , ImmutableArray<string> typeNamesToIgnore
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Pre-seed the set with ignored names so a single Add() covers both the ignore-list
                // check and deduplication in one pass.
                var seenTypeNames = new HashSet<string>(typeNamesToIgnore, StringComparer.Ordinal);
                using var valueTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                using var refTypeBuilder = ImmutableArrayBuilder<InternalVariantDeclaration>.Rent();
                var assemblyName = compilationCandidate.assemblyName;

                foreach (var candidate in candidates)
                {
                    if (seenTypeNames.Add(candidate.fullTypeName) == false)
                    {
                        continue;
                    }

                    MvvmInternalVariantWriteCode.WriteVariantCode(
                          ref context
                        , in candidate
                        , assemblyName
                        , outputSourceGenFiles
                        , s_errorDescriptor
                        , projectPath
                    );

                    if (candidate.isValueType)
                    {
                        valueTypeBuilder.Add(candidate);
                    }
                    else
                    {
                        refTypeBuilder.Add(candidate);
                    }
                }

                MvvmInternalVariantWriteCode.WriteStaticClass(
                      ref context
                    , valueTypeBuilder.ToImmutable()
                    , refTypeBuilder.ToImmutable()
                    , assemblyName
                    , outputSourceGenFiles
                    , s_errorDescriptor
                    , projectPath
                );
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_MVVM_INTERNAL_VARIANTS_01"
                , "Mvvm Internal Variant Generator Error"
                , "This error indicates a bug in the Mvvm Internal Variant source generators. Error message: '{0}'."
                , $"{NAMESPACE}.IObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}