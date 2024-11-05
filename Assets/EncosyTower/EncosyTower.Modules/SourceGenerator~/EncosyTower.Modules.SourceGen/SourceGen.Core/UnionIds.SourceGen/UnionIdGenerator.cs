using System;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.UnionIds.SourceGen
{
    [Generator]
    internal class UnionIdGenerator : IIncrementalGenerator
    {
        public const string UNION_ID_ATTRIBUTE = "global::EncosyTower.Modules.UnionIds.UnionIdAttribute";
        public const string UNION_ID_KIND_ATTRIBUTE = "global::EncosyTower.Modules.UnionIds.KindForUnionIdAttribute";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.UnionIds.SkipSourceGenForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(UnionIdGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var compilationProvider = context.CompilationProvider
                .Select(CompilationCandidate.GetCompilation);

            var idProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchId
                , transform: GetIdCandidate
            ).Where(static t => t.syntax is { } && t.symbol is { });

            var kindProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatchKind
                , transform: GetKindCandidate
            ).Where(static t => t.kindSymbol is { } && t.idSymbol is { });

            var combined = idProvider
                .Combine(kindProvider.Collect())
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

        private static bool IsSyntaxMatchId(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is StructDeclarationSyntax structSyntax
                && structSyntax.HasAttributeCandidate("EncosyTower.Modules", "UnionId");
        }

        private static bool IsSyntaxMatchKind(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is BaseTypeDeclarationSyntax typeSyntax
                && typeSyntax.Kind() is SyntaxKind.EnumDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordStructDeclaration
                && typeSyntax.GetAttribute("EncosyTower.Modules", "KindForUnionId") is AttributeSyntax attribSyntax
                && attribSyntax.ArgumentList != null
                && attribSyntax.ArgumentList.Arguments.Count > 0
                && attribSyntax.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax;
        }

        private static IdCandidate GetIdCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (symbol == null
                || symbol.IsUnmanagedType == false
                || symbol.IsUnboundGenericType
                || symbol.GetAttribute(UNION_ID_ATTRIBUTE) is not AttributeData attrib
            )
            {
                return default;
            }

            var candidate = new IdCandidate {
                syntax = syntax,
                symbol = symbol,
            };

            var args = attrib.NamedArguments;

            foreach (var arg in args)
            {
                switch (arg.Key)
                {
                    case "Size":
                    {
                        if (arg.Value.Value is byte byteVal)
                        {
                            candidate.size = (UnionIdSize)byteVal;
                        }
                        break;
                    }

                    case "DisplayNameForId":
                    {
                        if (arg.Value.Value is string stringVal)
                        {
                            candidate.displayNameForId = stringVal;
                        }
                        break;
                    }

                    case "DisplayNameForKind":
                    {
                        if (arg.Value.Value is string stringVal)
                        {
                            candidate.displayNameForKind = stringVal;
                        }
                        break;
                    }

                    case "KindSettings":
                    {
                        if (arg.Value.Value is byte byteVal)
                        {
                            candidate.kindSettings = (UnionIdKindSettings)byteVal;
                        }
                        break;
                    }
                }
            }

            return candidate;
        }

        private static KindCandidate GetKindCandidate(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not BaseTypeDeclarationSyntax syntax)
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var kindSymbol = semanticModel.GetDeclaredSymbol(syntax, token);

            if (kindSymbol == null
                || kindSymbol.IsUnmanagedType == false
                || kindSymbol.GetAttribute(UNION_ID_KIND_ATTRIBUTE) is not AttributeData attrib
                || attrib.ConstructorArguments.Length < 1
            )
            {
                return default;
            }

            var args = attrib.ConstructorArguments;
            var typeArg = args[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol idSymbol
                || idSymbol.IsUnmanagedType == false
                || idSymbol.IsUnboundGenericType
                || idSymbol.HasAttribute(UNION_ID_ATTRIBUTE) == false
            )
            {
                return default;
            }

            var candidate = new KindCandidate {
                kindSymbol = kindSymbol,
                idSymbol = idSymbol,
                attributeData = attrib,
            };

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.Kind != TypedConstantKind.Primitive)
                {
                    continue;
                }

                if (arg.Value is ulong ulongVal)
                {
                    candidate.order = ulongVal;
                }
                else if (arg.Value is string stringVal)
                {
                    candidate.displayName = stringVal;
                }
            }

            return candidate;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationCandidate compilationCandidate
            , IdCandidate idCandidate
            , ImmutableArray<KindCandidate> kindCandidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (idCandidate.syntax == null || idCandidate.symbol == null)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                SourceGenHelpers.ProjectPath = projectPath;

                var syntax = idCandidate.syntax;
                var symbol = idCandidate.symbol;
                var syntaxTree = syntax.SyntaxTree;
                var compilation = compilationCandidate.compilation;
                var assemblyName = compilation.Assembly.Name;

                var declaration = new UnionIdDeclaration(
                      context
                    , idCandidate
                    , kindCandidates
                    , compilationCandidate.references
                );

                if (declaration.IsInvalid)
                {
                    return;
                }

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
                    , idCandidate.syntax.GetLocation()
                    , e.ToUnityPrintableString()
                ));
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("SG_UNION_ID_01"
                , "UnionId Generator Error"
                , "This error indicates a bug in the UnionId source generators. Error message: '{0}'."
                , "EncosyTower.Modules.UnionIdAttribute"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
