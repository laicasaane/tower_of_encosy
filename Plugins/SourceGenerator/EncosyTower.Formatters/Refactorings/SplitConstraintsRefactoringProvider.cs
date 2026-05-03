using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Formatters.Formatting;
using EncosyTower.Formatters.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Refactorings
{
    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SplitConstraintsRefactoringProvider))]
    internal sealed class SplitConstraintsRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.SplitConstraints";

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

            if (ConstraintsFormatter.IsSingleLine(clause) == false)
            {
                return;
            }

            var style = EditorConfigSettings.GetCommaStyle(context.Document, clause.SyntaxTree);
            var title = style == SeparatorStyle.Leading
                ? "Split constraints into multiple lines (leading comma)"
                : "Split constraints into multiple lines (trailing comma)";

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: title
                    , createChangedDocument: c => SplitAsync(context.Document, clause, style, c)
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

        private static async Task<Document> SplitAsync(
              Document document
            , TypeParameterConstraintClauseSyntax clause
            , SeparatorStyle style
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var clauseIndent = RefactoringHelper.GetIndentBefore(clause);
            var newClause = ConstraintsFormatter.Split(clause, clauseIndent, style);

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
