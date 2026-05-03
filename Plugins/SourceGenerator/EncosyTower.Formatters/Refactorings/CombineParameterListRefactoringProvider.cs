using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Formatters.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace EncosyTower.Formatters.Refactorings
{
    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CombineParameterListRefactoringProvider))]
    internal sealed class CombineParameterListRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.Combine";
        private const string TITLE = "Combine into a single line";

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

            if (RefactoringHelper.TryFindList(root, context.Span, out var listNode) == false)
            {
                return;
            }

            if (ParameterListFormatter.TryGetListTokens(listNode, out var open, out var close, out var count) == false)
            {
                return;
            }

            if (count < 2)
            {
                return;
            }

            if (ParameterListFormatter.IsSingleLine(open, close))
            {
                return;
            }

            if (ParameterListFormatter.ListHasComment(listNode))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => CombineAsync(context.Document, listNode, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static async Task<Document> CombineAsync(
              Document document
            , SyntaxNode listNode
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var newList = ParameterListFormatter.CombineList(listNode, token);
            var newRoot = root.ReplaceNode(listNode, newList);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
