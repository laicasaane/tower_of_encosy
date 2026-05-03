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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SplitConditionalRefactoringProvider))]
    internal sealed class SplitConditionalRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.SplitConditional";
        private const string TITLE = "Split ternary into multiple lines";
        private const string INDENT_UNIT = "    ";

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

            if (TryFindConditional(root, context.Span, out var conditional) == false)
            {
                return;
            }

            if (OperatorChainFlattener.IsSingleLine(conditional) == false)
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => SplitAsync(context.Document, conditional, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static bool TryFindConditional(
              SyntaxNode root
            , Microsoft.CodeAnalysis.Text.TextSpan span
            , out ConditionalExpressionSyntax conditional
        )
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            while (node is not null)
            {
                if (node is ConditionalExpressionSyntax c)
                {
                    conditional = c;
                    return true;
                }

                node = node.Parent;
            }

            conditional = null;
            return false;
        }

        private static async Task<Document> SplitAsync(
              Document document
            , ConditionalExpressionSyntax conditional
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var baseIndent = RefactoringHelper.GetIndentBefore(conditional);
            var rebuilt = BuildSplit(conditional, baseIndent, depth: 1);
            var newRoot = root.ReplaceNode(conditional, rebuilt);
            return document.WithSyntaxRoot(newRoot);
        }

        private static ConditionalExpressionSyntax BuildSplit(
              ConditionalExpressionSyntax conditional
            , string baseIndent
            , int depth
        )
        {
            var indent = baseIndent;

            for (var i = 0; i < depth; i++)
            {
                indent += INDENT_UNIT;
            }

            var eol = TriviaUtil.Eol();
            var ws = TriviaUtil.Indent(indent);
            var space = TriviaUtil.Space();

            var newCondition = conditional.Condition.WithTrailingTrivia();
            var newQuestion = conditional.QuestionToken
                .WithLeadingTrivia(eol, ws)
                .WithTrailingTrivia(space);
            var newWhenTrue = conditional.WhenTrue
                .WithLeadingTrivia()
                .WithTrailingTrivia();
            var newColon = conditional.ColonToken
                .WithLeadingTrivia(eol, ws)
                .WithTrailingTrivia(space);

            ExpressionSyntax newWhenFalse;

            if (conditional.WhenFalse is ConditionalExpressionSyntax innerFalse)
            {
                var innerTrailing = innerFalse.GetTrailingTrivia();
                newWhenFalse = BuildSplit(innerFalse, baseIndent, depth + 1)
                    .WithLeadingTrivia()
                    .WithTrailingTrivia(innerTrailing);
            }
            else
            {
                newWhenFalse = conditional.WhenFalse
                    .WithLeadingTrivia()
                    .WithTrailingTrivia(conditional.WhenFalse.GetTrailingTrivia());
            }

            return SyntaxFactory.ConditionalExpression(
                  newCondition
                , newQuestion
                , newWhenTrue
                , newColon
                , newWhenFalse
            );
        }
    }
}
