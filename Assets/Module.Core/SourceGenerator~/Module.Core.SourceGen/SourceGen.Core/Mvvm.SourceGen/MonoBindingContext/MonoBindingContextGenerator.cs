using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Module.Core.SourceGen;

namespace Module.Core.Mvvm.MonoBindingContextSourceGen
{
    [Generator]
    public class MonoBindingContextGenerator : IIncrementalGenerator
    {
        public const string COMPONENT = "global::UnityEngine.Component";
        public const string MONO_BEHAVIOUR = "global::UnityEngine.MonoBehaviour";
        public const string GENERATOR_NAME = nameof(MonoBindingContextGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsClassSyntaxMatch,
                transform: GetClassSemanticMatch
            ).Where(static t => t is { });

            var combined = candidateProvider
                .Combine(context.CompilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.IsValidCompilation(MvvmGeneratorHelpers.SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        public static bool IsClassSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax classSyntax
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0
                && classSyntax.BaseList.Types.Any(
                    static x => x.Type.IsTypeNameCandidate("Module.Core.Mvvm.ComponentModel", "IObservableObject")
                );
        }

        public static ClassDeclarationSyntax GetClassSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.BaseList.Types.Count < 1
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var typeSymbol = semanticModel.GetDeclaredSymbol(classSyntax, token);

            if (typeSymbol.IsAbstract || typeSymbol.IsUnboundGenericType)
            {
                return null;
            }

            ITypeSymbol baseTypeSymbol = null;

            foreach (var baseType in classSyntax.BaseList.Types)
            {
                if (baseType.IsTypeNameCandidate("UnityEngine", "MonoBehaviour"))
                {
                    return classSyntax;
                }

                if (baseType.IsTypeNameCandidate("UnityEngine", "Component"))
                {
                    return classSyntax;
                }

                var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                if (typeInfo.Type is ITypeSymbol type
                    && type.TypeKind == TypeKind.Class
                )
                {
                    baseTypeSymbol = type.BaseType;
                    break;
                }
            }

            while (baseTypeSymbol != null)
            {
                if (baseTypeSymbol.Is(MONO_BEHAVIOUR, false) || baseTypeSymbol.Is(COMPONENT, false))
                {
                    return classSyntax;
                }

                baseTypeSymbol = baseTypeSymbol.BaseType;
            }

            return null;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ClassDeclarationSyntax candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntaxTree = candidate.SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var declaration = new MonoBindingContextDeclaration(candidate, semanticModel, context.CancellationToken);
                var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(compilation.Assembly.Name, GENERATOR_NAME);
                var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                      sourceFilePath
                    , candidate
                    , declaration.WriteCode()
                    , context.CancellationToken
                );

                context.AddSource(
                      syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, candidate, declaration.Symbol.ToValidIdentifier())
                    , outputSource
                );

                if (outputSourceGenFiles)
                {
                    SourceGenHelpers.OutputSourceToFile(
                          context
                        , candidate.GetLocation()
                        , sourceFilePath
                        , outputSource
                    );
                }
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      s_errorDescriptor
                    , candidate.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_MONO_BINDING_CONTEXT_01"
                , "Mono Binding Context Generator Error"
                , "This error indicates a bug in the Mono Binding Context source generators. Error message: '{0}'."
                , "Module.Core.Mvvm.IObservableObject"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}