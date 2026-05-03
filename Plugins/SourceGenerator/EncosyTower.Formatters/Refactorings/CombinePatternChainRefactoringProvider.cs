using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Formatters.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Refactorings
{
    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CombinePatternChainRefactoringProvider))]
    internal sealed class CombinePatternChainRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.CombinePatternChain";
        private const string TITLE = "Combine pattern chain into a single line";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var token = context.CancellationToken;

            var root = await context.Document
                .GetSyntaxRootAsync(token)
                .ConfigureAwait(false);

            if (root is null)
            {
                return;
            }

            if (TryFindChainRoot(root, context.Span, out var chainRoot) == false)
            {
                return;
            }

            if (OperatorChainFlattener.TryFlatten(chainRoot, out var items, out var ops) == false)
            {
                return;
            }

            if (OperatorChainFlattener.IsSingleLine(chainRoot))
            {
                return;
            }

            if (BinaryChainFormatter.ChainHasCommentPattern(items, ops))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => CombineAsync(context.Document, chainRoot, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static bool TryFindChainRoot(
              SyntaxNode root
            , Microsoft.CodeAnalysis.Text.TextSpan span
            , out BinaryPatternSyntax chainRoot
        )
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            while (node is not null)
            {
                if (node is BinaryPatternSyntax bin && IsBinaryPatternKind(bin.Kind()))
                {
                    chainRoot = OperatorChainFlattener.FindBinaryPatternChainRoot(bin);
                    return true;
                }

                node = node.Parent;
            }

            chainRoot = null;
            return false;
        }

        private static bool IsBinaryPatternKind(SyntaxKind kind)
        {
            return kind == SyntaxKind.AndPattern
                || kind == SyntaxKind.OrPattern;
        }

        private static async Task<Document> CombineAsync(
              Document document
            , BinaryPatternSyntax chainRoot
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            if (OperatorChainFlattener.TryFlatten(chainRoot, out var items, out var ops) == false)
            {
                return document;
            }

            var rebuilt = BinaryChainFormatter.CombinePattern(chainRoot, items, ops);
            var newRoot = root.ReplaceNode(chainRoot, rebuilt);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
