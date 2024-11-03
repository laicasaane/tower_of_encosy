using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.NewtonsoftAotHelpers.SourceGen
{
    [Generator]
    internal class NewtonsoftAotHelperGenerator : IIncrementalGenerator
    {
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.NewtonsoftAot.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(NewtonsoftAotHelperGenerator);
        private const string ATTRIBUTE = "global::Newtonsoft.Json.Utilities.NewtonsoftAotHelperAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var helperProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchProviderClass
                , transform: GetHelperCandidate
            ).Where(static t => t.syntax is { } && t.symbol is { } && t.baseType is { });

            var typeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchType
                , transform: GetType
            ).Where(static t => t is { });

            var combined = helperProvider
                .Combine(typeProvider.Collect())
                .Combine(compilationProvider)
                .Combine(projectPathProvider)
                .Where(static t => t.Left.Right.compilation.IsValidCompilation(SKIP_ATTRIBUTE));

            context.RegisterSourceOutput(combined, (sourceProductionContext, source) => {
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

        private static bool IsSyntaxMatchProviderClass(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is TypeDeclarationSyntax syntax
                && syntax.Kind() is (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                && syntax.HasAttributeCandidate("Newtonsoft.Json.Utilities", "NewtonsoftAotHelper");
        }

        private static HelperCandidate GetHelperCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not TypeDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
                || symbol.GetAttribute(ATTRIBUTE) is not AttributeData attrib
            )
            {
                return default;
            }

            var args = attrib.ConstructorArguments;

            if (args.Length != 1 || args[0].Value is not ITypeSymbol baseType)
            {
                return default;
            }

            return new HelperCandidate {
                syntax = syntax,
                symbol = symbol,
                baseType = baseType,
            };
        }

        public static bool IsSyntaxMatchType(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is TypeDeclarationSyntax syntax
                && syntax.Kind() is (SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration)
                && syntax.BaseList != null
                && syntax.BaseList.Types.Count > 0;
        }

        public static INamedTypeSymbol GetType(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
                || context.Node is not TypeDeclarationSyntax syntax
                || syntax.BaseList == null
                || syntax.BaseList.Types.Count < 1
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
            )
            {
                return default;
            }

            var constructors = symbol.Constructors;

            foreach (var constructor in constructors)
            {
                if (constructor.Parameters.Length == 0)
                {
                    return symbol;
                }
            }

            return default;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , HelperCandidate helperCandidate
            , ImmutableArray<INamedTypeSymbol> typeCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (compilationCandidate.compilation == null
                || helperCandidate.syntax == null
                || helperCandidate.symbol == null
                || helperCandidate.baseType == null
                || typeCandidates.Length < 1
            )
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                var types = new List<INamedTypeSymbol>(typeCandidates.Length);
                var sb = new StringBuilder();
                var baseType = helperCandidate.baseType;
                var equalityComparer = SymbolEqualityComparer.Default;

                for (var i = 0; i < typeCandidates.Length; i++)
                {
                    var candidate = typeCandidates[i];
                    var candidateBaseType = candidate;

                    while (candidateBaseType != null)
                    {
                        if (equalityComparer.Equals(candidateBaseType, baseType))
                        {
                            types.Add(candidate);
                            break;
                        }

                        candidateBaseType = candidateBaseType.BaseType;
                    }
                }

                var helperSyntax = helperCandidate.syntax;
                var helperSymbol = helperCandidate.symbol;
                var syntaxTree = helperSyntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                SourceGenHelpers.ProjectPath = projectPath;

                context.OutputSource(
                      outputSourceGenFiles
                    , helperSyntax
                    , new HelperDeclaration().WriteCode(helperCandidate, types)
                    , syntaxTree.GetGeneratedSourceFileName(GENERATOR_NAME, helperSyntax, helperSymbol.ToValidIdentifier())
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
                    , helperCandidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_NEWTONSOFT_AOT_HELPER_01"
                , "NewtonsoftAotHelper Generator Error"
                , "This error indicates a bug in the NewtonsoftAotHelper source generators. Error message: '{0}'."
                , "Newtonsoft.Json.Utilities.NewtonsoftAotHelperAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
