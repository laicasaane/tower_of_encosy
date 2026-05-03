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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CombineConditionalRefactoringProvider))]
    internal sealed class CombineConditionalRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.CombineConditional";
        private const string TITLE = "Combine ternary into a single line";

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

            if (OperatorChainFlattener.IsSingleLine(conditional))
            {
                return;
            }

            if (HasComment(conditional))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => CombineAsync(context.Document, conditional, c)
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

        private static bool HasComment(ConditionalExpressionSyntax conditional)
        {
            if (TriviaUtil.TriviaListHasComment(conditional.Condition.GetLeadingTrivia())
                || TriviaUtil.TriviaListHasComment(conditional.Condition.GetTrailingTrivia())
                || TriviaUtil.TriviaListHasComment(conditional.QuestionToken.LeadingTrivia)
                || TriviaUtil.TriviaListHasComment(conditional.QuestionToken.TrailingTrivia)
                || TriviaUtil.TriviaListHasComment(conditional.WhenTrue.GetLeadingTrivia())
                || TriviaUtil.TriviaListHasComment(conditional.WhenTrue.GetTrailingTrivia())
                || TriviaUtil.TriviaListHasComment(conditional.ColonToken.LeadingTrivia)
                || TriviaUtil.TriviaListHasComment(conditional.ColonToken.TrailingTrivia)
                || TriviaUtil.TriviaListHasComment(conditional.WhenFalse.GetLeadingTrivia())
                || TriviaUtil.TriviaListHasComment(conditional.WhenFalse.GetTrailingTrivia())
            )
            {
                return true;
            }

            return false;
        }

        private static async Task<Document> CombineAsync(
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

            var space = TriviaUtil.Space();

            var newCondition = conditional.Condition.WithTrailingTrivia();
            var newQuestion = conditional.QuestionToken
                .WithLeadingTrivia(space)
                .WithTrailingTrivia(space);
            var newWhenTrue = conditional.WhenTrue
                .WithLeadingTrivia()
                .WithTrailingTrivia();
            var newColon = conditional.ColonToken
                .WithLeadingTrivia(space)
                .WithTrailingTrivia(space);
            var newWhenFalse = conditional.WhenFalse
                .WithLeadingTrivia()
                .WithTrailingTrivia(conditional.WhenFalse.GetTrailingTrivia());

            var rebuilt = SyntaxFactory.ConditionalExpression(
                  newCondition
                , newQuestion
                , newWhenTrue
                , newColon
                , newWhenFalse
            );

            var newRoot = root.ReplaceNode(conditional, rebuilt);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
