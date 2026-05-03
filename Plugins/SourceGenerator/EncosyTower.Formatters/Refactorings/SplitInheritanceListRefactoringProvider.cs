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
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SplitInheritanceListRefactoringProvider))]
    internal sealed class SplitInheritanceListRefactoringProvider : CodeRefactoringProvider
    {
        private const string EQUIVALENCE_KEY = "EncosyTower.Formatters.SplitInheritance";

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

            if (baseList.Types.Count < 2)
            {
                return;
            }

            if (BaseListFormatter.IsSingleLine(baseList) == false)
            {
                return;
            }

            var style = EditorConfigSettings.GetInheritanceStyle(context.Document, baseList.SyntaxTree);
            var title = style == SeparatorStyle.Leading
                ? "Split inheritance list into multiple lines (leading)"
                : "Split inheritance list into multiple lines (trailing)";

            context.RegisterRefactoring(
                CodeAction.Create(
                      title: title
                    , createChangedDocument: c => SplitAsync(context.Document, baseList, style, c)
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

        private static async Task<Document> SplitAsync(
              Document document
            , BaseListSyntax baseList
            , SeparatorStyle style
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
                var rebuiltOnly = BaseListFormatter.Split(baseList, RefactoringHelper.GetIndentBefore(baseList), style);
                return document.WithSyntaxRoot(root.ReplaceNode(baseList, rebuiltOnly));
            }

            var baseIndent = RefactoringHelper.GetIndentBefore(typeDecl);
            var newBaseList = BaseListFormatter.Split(baseList, baseIndent, style);
            var prevToken = baseList.GetFirstToken().GetPreviousToken();
            var newPrevToken = RefactoringHelper.WithoutTrailingWhitespace(prevToken);
            var newOpenBrace = RefactoringHelper.WithBraceLeadingOwnLine(typeDecl.OpenBraceToken, baseIndent);

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
