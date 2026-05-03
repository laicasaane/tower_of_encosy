using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Formatters.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Refactorings
{
    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CombineConstraintsRefactoringProvider))]
    internal sealed class CombineConstraintsRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.CombineConstraints";
        private const string TITLE = "Combine constraints into a single line";

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

            if (TryFindClause(root, context.Span, out var clause) == false)
            {
                return;
            }

            if (clause.Constraints.Count < 2)
            {
                return;
            }

            if (ConstraintsFormatter.IsSingleLine(clause))
            {
                return;
            }

            if (ConstraintsFormatter.HasComment(clause))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => CombineAsync(context.Document, clause, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static bool TryFindClause(
              SyntaxNode root
            , Microsoft.CodeAnalysis.Text.TextSpan span
            , out TypeParameterConstraintClauseSyntax clause
        )
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            while (node is not null)
            {
                if (node is TypeParameterConstraintClauseSyntax c)
                {
                    clause = c;
                    return true;
                }

                node = node.Parent;
            }

            clause = null;
            return false;
        }

        private static async Task<Document> CombineAsync(
              Document document
            , TypeParameterConstraintClauseSyntax clause
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var newClause = ConstraintsFormatter.Combine(clause);

            var typeDecl = clause.Parent as TypeDeclarationSyntax;

            if (typeDecl is null)
            {
                return document.WithSyntaxRoot(root.ReplaceNode(clause, newClause));
            }

            var declIndent = RefactoringHelper.GetIndentBefore(typeDecl);
            var newOpenBrace = RefactoringHelper.WithBraceLeadingOwnLine(typeDecl.OpenBraceToken, declIndent);

            var newDecl = typeDecl.ReplaceSyntax(
                  nodes: new[] { (SyntaxNode)clause }
                , computeReplacementNode: (orig, _) => newClause
                , tokens: new[] { typeDecl.OpenBraceToken }
                , computeReplacementToken: (orig, _) => orig == typeDecl.OpenBraceToken ? newOpenBrace : orig
                , trivia: null
                , computeReplacementTrivia: null
            );

            var newRoot = root.ReplaceNode(typeDecl, newDecl);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
