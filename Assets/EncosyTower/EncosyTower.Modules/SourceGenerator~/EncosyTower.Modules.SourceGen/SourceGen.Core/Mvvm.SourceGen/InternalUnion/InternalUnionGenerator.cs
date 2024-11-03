using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.InternalUnionSourceGen
{
    [Generator]
    public class InternalUnionGenerator : IIncrementalGenerator
    {
        private const string COMPONENT_MODEL_NS = "global::EncosyTower.Modules.Mvvm.ComponentModel";
        private const string VIEW_BINDING_NS = "EncosyTower.Modules.Mvvm.ViewBinding";
        private const string INPUT_NS = "EncosyTower.Modules.Mvvm.Input";
        private const string IOBSERVABLE_OBJECT = "global::EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject";
        private const string IUNION_T = "global::EncosyTower.Modules.Unions.IUnion<";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsSyntaxMatch,
                transform: GetSemanticMatch
            ).Where(static t => t is { });

            var candidateToIgnoreProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsStructSyntaxMatch,
                transform: GetTypeInGenericUnionDeclaration
            ).Where(static t => t is { });

            var combined = candidateProvider.Collect()
                .Combine(candidateToIgnoreProvider.Collect())
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
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
                    static x => x.Type.IsTypeNameGenericCandidate("EncosyTower.Modules.Unions", "IUnion", out _)
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

        public static TypeRef GetSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE) == false)
            {
                return null;
            }

            var semanticModel = context.SemanticModel;

            if (context.Node is FieldDeclarationSyntax field)
            {
                return new TypeRef {
                    Syntax = field.Declaration.Type,
                    Symbol = semanticModel.GetTypeInfo(field.Declaration.Type, token).Type,
                };
            }

            if (context.Node is MethodDeclarationSyntax method)
            {
                var typeSyntax = method.ParameterList.Parameters[0].Type;

                return new TypeRef {
                    Syntax = typeSyntax,
                    Symbol = semanticModel.GetTypeInfo(typeSyntax, token).Type,
                };
            }

            if (context.Node is PropertyDeclarationSyntax property
                && property.Parent is ClassDeclarationSyntax classSyntax
                && classSyntax.DoesSemanticMatch(IOBSERVABLE_OBJECT, context.SemanticModel, token)
                && classSyntax.AnyFieldHasNotifyPropertyChangedForAttribute(property)
            )
            {
                return new TypeRef {
                    Syntax = property.Type,
                    Symbol = semanticModel.GetTypeInfo(property.Type, token).Type,
                };
            }

            return null;
        }

        public static ITypeSymbol GetTypeInGenericUnionDeclaration(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE) == false
                || context.Node is not StructDeclarationSyntax structSyntax
                || structSyntax.BaseList == null
                || structSyntax.BaseList.Types.Count < 1
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;

            foreach (var baseType in structSyntax.BaseList.Types)
            {
                var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                if (typeInfo.Type is INamedTypeSymbol interfaceSymbol)
                {
                    if (interfaceSymbol.IsGenericType
                       && interfaceSymbol.TypeParameters.Length == 1
                       && interfaceSymbol.ToFullName().StartsWith(IUNION_T)
                    )
                    {
                        return interfaceSymbol.TypeArguments[0];
                    }
                }

                if (TryGetMatchTypeArgument(typeInfo.Type.Interfaces, out var type)
                    || TryGetMatchTypeArgument(typeInfo.Type.AllInterfaces, out type)
                )
                {
                    return type;
                }
            }

            return null;

            static bool TryGetMatchTypeArgument(
                  ImmutableArray<INamedTypeSymbol> interfaces
                , out ITypeSymbol result
            )
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.IsGenericType
                        && interfaceSymbol.TypeParameters.Length == 1
                        && interfaceSymbol.ToFullName().StartsWith(IUNION_T)
                    )
                    {
                        result = interfaceSymbol.TypeArguments[0];
                        return true;
                    }
                }

                result = default;
                return false;
            }
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ImmutableArray<TypeRef> candidates
            , ImmutableArray<ITypeSymbol> candidatesToIgnore
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
                SourceGenHelpers.ProjectPath = projectPath;

                var declaration = new InternalUnionDeclaration(candidates, candidatesToIgnore);

                declaration.GenerateUnionForValueTypes(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );

                declaration.GenerateUnionForRefTypes(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );

                declaration.GenerateStaticClass(
                      context
                    , compilation
                    , outputSourceGenFiles
                    , s_errorDescriptor
                );
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , null
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_INTERNAL_UNIONS_01"
                , "Internal Union Generator Error"
                , "This error indicates a bug in the Internal Union source generators. Error message: '{0}'."
                , "EncosyTower.Modules.Mvvm.IObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
