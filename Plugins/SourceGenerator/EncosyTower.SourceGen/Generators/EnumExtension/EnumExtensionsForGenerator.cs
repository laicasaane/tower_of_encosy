using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    [Generator]
    public class EnumExtensionsForGenerator : IIncrementalGenerator
    {
        private const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string ENUM_EXTENSIONS_FOR_ATTRIBUTE = $"global::{NAMESPACE}.EnumExtensionsForAttribute";
        public const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";
        public const string GENERATOR_NAME = nameof(EnumExtensionsGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(static (x, _) => CompilationCandidateSlim.GetCompilation(x, NAMESPACE, SKIP_ATTRIBUTE));

            var candidateProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidClassSyntax,
                transform: GetSemanticSymbolMatch
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var combined = candidateProvider
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.isValid);

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

        private static bool IsValidClassSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is ClassDeclarationSyntax syntax
                && syntax.HasModifier(SyntaxKind.StaticKeyword)
                && syntax.AttributeLists.Count > 0
                && syntax.HasAttributeCandidate(NAMESPACE, "EnumExtensionsFor")
                ;
        }

        private static MatchedSemantic GetSemanticSymbolMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax syntax
                || syntax.HasModifier(SyntaxKind.StaticKeyword) == false
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (classSymbol == null)
            {
                return default;
            }

            var attribute = classSymbol.GetAttribute(ENUM_EXTENSIONS_FOR_ATTRIBUTE);

            if (attribute == null || attribute.ConstructorArguments.Length < 1)
            {
                return default;
            }

            INamedTypeSymbol symbol = null;

            foreach (var attribList in syntax.AttributeLists)
            {
                foreach (var attrib in attribList.Attributes)
                {
                    if (attrib.Name.IsTypeNameCandidate(NAMESPACE, "EnumExtensionsFor") == false
                        || attrib.ArgumentList == null
                        || attrib.ArgumentList.Arguments.Count < 1
                    )
                    {
                        continue;
                    }

                    var arg = attrib.ArgumentList.Arguments[0];
                    var expr = arg.Expression;

                    if (expr is TypeOfExpressionSyntax typeOfExpr)
                    {
                        var candidate = semanticModel.GetSymbolInfo(typeOfExpr.Type, token).Symbol as INamedTypeSymbol;

                        if (candidate.TypeKind == TypeKind.Enum)
                        {
                            symbol = candidate;
                        }
                    }

                    if (symbol != null)
                    {
                        break;
                    }
                }

                if (symbol != null)
                {
                    break;
                }
            }

            return new MatchedSemantic {
                syntax = syntax,
                symbol = symbol,
                accessibility = classSymbol.DeclaredAccessibility,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidateSlim compilation
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

                var declaration = new EnumExtensionsDeclaration(
                      candidate.symbol
                    , candidate.syntax.Parent is BaseNamespaceDeclarationSyntax
                    , candidate.syntax.Identifier.Text
                    , candidate.accessibility
                    , compilation.references.unityCollections
                );

                context.OutputSource(
                      outputSourceGenFiles
                    , syntax
                    , declaration.WriteCode()
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, syntax, symbol.ToValidIdentifier())
                    , syntaxTree.GetGeneratedSourceFilePath(compilation.assemblyName, GENERATOR_NAME)
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
            = new("SG_ENUM_EXTENSIONS_02"
                , "Enum Extensions For Generator Error"
                , "This error indicates a bug in the Enum Extensions For source generators. Error message: '{0}'."
                , $"{NAMESPACE}.EnumExtensionsForAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );

        private struct MatchedSemantic
        {
            public ClassDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
            public Accessibility accessibility;
        }
    }
}
