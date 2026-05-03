using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Formatters.Formatting;
using EncosyTower.Formatters.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace EncosyTower.Formatters.Refactorings
{
    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SplitParameterListRefactoringProvider))]
    internal sealed class SplitParameterListRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.Split";

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

            if (ParameterListFormatter.IsSingleLine(open, close) == false)
            {
                return;
            }

            var style = EditorConfigSettings.GetCommaStyle(context.Document, listNode.SyntaxTree);
            var title = style == SeparatorStyle.Leading
                ? "Split into multiple lines (leading comma)"
                : "Split into multiple lines (trailing comma)";

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: title
                    , createChangedDocument: c => SplitAsync(context.Document, listNode, style, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static async Task<Document> SplitAsync(
              Document document
            , SyntaxNode listNode
            , SeparatorStyle style
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var baseIndent = RefactoringHelper.GetBaseIndent(listNode);
            var newList = ParameterListFormatter.SplitList(listNode, baseIndent, style, token);
            var newRoot = root.ReplaceNode(listNode, newList);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
