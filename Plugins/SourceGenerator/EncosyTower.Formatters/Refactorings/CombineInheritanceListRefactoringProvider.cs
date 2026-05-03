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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CombineInheritanceListRefactoringProvider))]
    internal sealed class CombineInheritanceListRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.CombineInheritance";
        private const string TITLE = "Combine inheritance list into a single line";

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

            if (TryFindBaseList(root, context.Span, out var baseList) == false)
            {
                return;
            }

            if (BaseListFormatter.IsSingleLine(baseList))
            {
                return;
            }

            if (BaseListFormatter.HasComment(baseList))
            {
                return;
            }

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: TITLE
                    , createChangedDocument: c => CombineAsync(context.Document, baseList, c)
                    , equivalenceKey: EQUIVALENCE_KEY
                )
            );
        }

        private static bool TryFindBaseList(
              SyntaxNode root
            , Microsoft.CodeAnalysis.Text.TextSpan span
            , out BaseListSyntax baseList
        )
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            while (node is not null)
            {
                if (node is BaseListSyntax bl)
                {
                    baseList = bl;
                    return true;
                }

                node = node.Parent;
            }

            baseList = null;
            return false;
        }

        private static async Task<Document> CombineAsync(
              Document document
            , BaseListSyntax baseList
            , CancellationToken token
        )
        {
            var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            if (root is null)
            {
                return document;
            }

            var typeDecl = baseList.Parent as TypeDeclarationSyntax;

            if (typeDecl is null)
            {
                var rebuiltOnly = BaseListFormatter.Combine(baseList);
                return document.WithSyntaxRoot(root.ReplaceNode(baseList, rebuiltOnly));
            }

            var newBaseList = BaseListFormatter.Combine(baseList);
            var prevToken = baseList.GetFirstToken().GetPreviousToken();
            var newPrevToken = RefactoringHelper.WithoutTrailingWhitespace(prevToken);
            var newOpenBrace = RefactoringHelper.WithBraceLeadingSameLine(typeDecl.OpenBraceToken);

            var newDecl = typeDecl.ReplaceSyntax(
                  nodes: new[] { (SyntaxNode)baseList }
                , computeReplacementNode: (orig, _) => newBaseList
                , tokens: new[] { prevToken, typeDecl.OpenBraceToken }
                , computeReplacementToken: (orig, _) =>
                {
                    if (orig == prevToken)
                    {
                        return newPrevToken;
                    }

                    if (orig == typeDecl.OpenBraceToken)
                    {
                        return newOpenBrace;
                    }

                    return orig;
                }
                , trivia: null
                , computeReplacementTrivia: null
            );

            var newRoot = root.ReplaceNode(typeDecl, newDecl);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
