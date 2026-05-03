using EncosyTower.Formatters.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.Formatters.Refactorings
{
    internal static class RefactoringHelper
    {
        public static SyntaxToken WithoutTrailingWhitespace(SyntaxToken token)
        {
            var keep = TriviaUtil.KeepCommentsOnly(token.TrailingTrivia);
            return token.WithTrailingTrivia(keep);
        }

        public static SyntaxToken WithBraceLeadingOwnLine(SyntaxToken brace, string indent)
        {
            return brace.WithLeadingTrivia(TriviaUtil.Eol(), TriviaUtil.Indent(indent));
        }

        public static SyntaxToken WithBraceLeadingSameLine(SyntaxToken brace)
        {
            return brace.WithLeadingTrivia(TriviaUtil.Space());
        }

        public static bool TryFindList(SyntaxNode root, TextSpan span, out SyntaxNode listNode)
        {
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            while (node is not null)
            {
                if (IsTargetList(node) && SpanTouchesList(node, span))
                {
                    listNode = node;
                    return true;
                }

                node = node.Parent;
            }

            listNode = null;
            return false;
        }

        public static string GetBaseIndent(SyntaxNode listNode)
        {
            var anchor = FindIndentAnchor(listNode);

            if (anchor is null)
            {
                return string.Empty;
            }

            var leading = anchor.GetLeadingTrivia();
            var lastEol = -1;

            for (var i = 0; i < leading.Count; i++)
            {
                if (leading[i].IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    lastEol = i;
                }
            }

            var sb = new System.Text.StringBuilder();

            for (var i = lastEol + 1; i < leading.Count; i++)
            {
                if (leading[i].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    sb.Append(leading[i].ToString());
                }
            }

            return sb.ToString();
        }

        private static SyntaxNode FindIndentAnchor(SyntaxNode listNode)
        {
            var node = listNode.Parent;

            while (node is not null)
            {
                if (node is MemberDeclarationSyntax or StatementSyntax)
                {
                    return node;
                }

                node = node.Parent;
            }

            return listNode;
        }

        public static string GetIndentBefore(SyntaxNode node)
        {
            var current = node;

            while (current is not null)
            {
                var leading = current.GetLeadingTrivia();
                var indent = ExtractTrailingWhitespace(leading);

                if (indent is not null)
                {
                    return indent;
                }

                current = current.Parent;
            }

            return string.Empty;
        }

        private static string ExtractTrailingWhitespace(SyntaxTriviaList leading)
        {
            var count = leading.Count;
            var lastEol = -1;

            for (var i = 0; i < count; i++)
            {
                if (leading[i].IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    lastEol = i;
                }
            }

            if (lastEol < 0)
            {
                return null;
            }

            var sb = new System.Text.StringBuilder();

            for (var i = lastEol + 1; i < count; i++)
            {
                if (leading[i].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    sb.Append(leading[i].ToString());
                }
            }

            return sb.ToString();
        }

        private static bool IsTargetList(SyntaxNode node)
        {
            return node is ParameterListSyntax
                or BracketedParameterListSyntax
                or ArgumentListSyntax
                or BracketedArgumentListSyntax
                or AttributeArgumentListSyntax;
        }

        private static bool SpanTouchesList(SyntaxNode listNode, TextSpan span)
        {
            return listNode.FullSpan.IntersectsWith(span);
        }
    }
}
