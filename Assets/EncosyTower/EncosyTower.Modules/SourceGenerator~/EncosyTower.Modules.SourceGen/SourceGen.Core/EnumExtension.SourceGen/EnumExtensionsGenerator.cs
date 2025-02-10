using System;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.EnumExtensions.SourceGen
{
    [Generator]
    public class EnumExtensionsGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.Modules.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGenForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidEnumSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(NAMESPACE, SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidEnumSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is EnumDeclarationSyntax syntax
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate(NAMESPACE, "EnumExtensions")
                ;
        }

        private static MatchedSemantic GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not EnumDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null || symbol.HasAttribute(ENUM_EXTENSIONS_ATTRIBUTE) == false)
            {
                return default;
            }

            return new MatchedSemantic {
                syntax = syntax,
                symbol = symbol,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , MatchedSemantic candidate
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidate.syntax == null || candidate.symbol == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntax = candidate.syntax;
                var symbol = candidate.symbol;
                var syntaxTree = syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                var declaration = new EnumExtensionsDeclaration(
                      candidate.symbol
                    , candidate.syntax.Parent is NamespaceDeclarationSyntax
                    , $"{candidate.symbol.Name}Extensions"
                    , candidate.symbol.DeclaredAccessibility
                    , compilationCandidate.references.unityCollections
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , declaration.WriteCode()
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, symbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(assemblyName, GENERATOR_NAME)
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
                    , candidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_ENUM_EXTENSIONS_01"
                , "Enum Extensions Generator Error"
                , "This error indicates a bug in the Enum Extensions source generators. Error message: '{0}'."
                , $"{NAMESPACE}.EnumExtensionsAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private struct MatchedSemantic
        {
            public EnumDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
        }
    }
}
