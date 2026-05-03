using System.Collections.Generic;
using System.Threading;
using EncosyTower.Formatters.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Formatting
{
    internal static class ParameterListFormatter
    {
        private const string INDENT_UNIT = "    ";
        private const string LEADING_FIRST_EXTRA = "  ";

        public static SyntaxNode SplitList(
              SyntaxNode listNode
            , string baseIndent
            , CommaStyle style
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            return listNode switch {
                ParameterListSyntax pl => SplitParameterList(pl, baseIndent, style),
                BracketedParameterListSyntax bpl => SplitBracketedParameterList(bpl, baseIndent, style),
                ArgumentListSyntax al => SplitArgumentList(al, baseIndent, style),
                BracketedArgumentListSyntax bal => SplitBracketedArgumentList(bal, baseIndent, style),
                AttributeArgumentListSyntax aal => SplitAttributeArgumentList(aal, baseIndent, style),
                _ => listNode,
            };
        }

        public static SyntaxNode CombineList(SyntaxNode listNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return listNode switch {
                ParameterListSyntax pl => CombineParameterList(pl),
                BracketedParameterListSyntax bpl => CombineBracketedParameterList(bpl),
                ArgumentListSyntax al => CombineArgumentList(al),
                BracketedArgumentListSyntax bal => CombineBracketedArgumentList(bal),
                AttributeArgumentListSyntax aal => CombineAttributeArgumentList(aal),
                _ => listNode,
            };
        }

        public static bool IsSingleLine(SyntaxToken open, SyntaxToken close)
        {
            var tree = open.SyntaxTree;

            if (tree is null)
            {
                return true;
            }

            var openLine = tree.GetLineSpan(open.Span).StartLinePosition.Line;
            var closeLine = tree.GetLineSpan(close.Span).StartLinePosition.Line;
            return openLine == closeLine;
        }

        public static bool ListHasComment(SyntaxNode listNode)
        {
            if (TryGetListTokens(listNode, out var open, out var close, out _) == false)
            {
                return false;
            }

            if (TriviaListHasComment(open.LeadingTrivia) || TriviaListHasComment(open.TrailingTrivia))
            {
                return true;
            }

            if (TriviaListHasComment(close.LeadingTrivia) || TriviaListHasComment(close.TrailingTrivia))
            {
                return true;
            }

            var separated = GetSeparatedNodesAndTokens(listNode);
            var count = separated.Count;

            for (var i = 0; i < count; i++)
            {
                var nt = separated[i];

                if (nt.IsNode)
                {
                    var item = nt.AsNode();

                    if (TriviaListHasComment(item.GetLeadingTrivia())
                        || TriviaListHasComment(item.GetTrailingTrivia())
                    )
                    {
                        return true;
                    }
                }
                else
                {
                    var tok = nt.AsToken();

                    if (TriviaListHasComment(tok.LeadingTrivia) || TriviaListHasComment(tok.TrailingTrivia))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static SyntaxNodeOrTokenList GetSeparatedNodesAndTokens(SyntaxNode listNode)
        {
            return listNode switch {
                ParameterListSyntax pl => pl.Parameters.GetWithSeparators(),
                BracketedParameterListSyntax bpl => bpl.Parameters.GetWithSeparators(),
                ArgumentListSyntax al => al.Arguments.GetWithSeparators(),
                BracketedArgumentListSyntax bal => bal.Arguments.GetWithSeparators(),
                AttributeArgumentListSyntax aal => aal.Arguments.GetWithSeparators(),
                _ => default,
            };
        }

        private static bool TriviaListHasComment(SyntaxTriviaList trivia)
        {
            for (var i = 0; i < trivia.Count; i++)
            {
                if (IsCommentTrivia(trivia[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetListTokens(
              SyntaxNode listNode
            , out SyntaxToken open
            , out SyntaxToken close
            , out int itemCount
        )
        {
            switch (listNode)
            {
                case ParameterListSyntax pl:
                    open = pl.OpenParenToken;
                    close = pl.CloseParenToken;
                    itemCount = pl.Parameters.Count;
                    return true;

                case BracketedParameterListSyntax bpl:
                    open = bpl.OpenBracketToken;
                    close = bpl.CloseBracketToken;
                    itemCount = bpl.Parameters.Count;
                    return true;

                case ArgumentListSyntax al:
                    open = al.OpenParenToken;
                    close = al.CloseParenToken;
                    itemCount = al.Arguments.Count;
                    return true;

                case BracketedArgumentListSyntax bal:
                    open = bal.OpenBracketToken;
                    close = bal.CloseBracketToken;
                    itemCount = bal.Arguments.Count;
                    return true;

                case AttributeArgumentListSyntax aal:
                    open = aal.OpenParenToken;
                    close = aal.CloseParenToken;
                    itemCount = aal.Arguments.Count;
                    return true;
            }

            open = default;
            close = default;
            itemCount = 0;
            return false;
        }

        private static ParameterListSyntax SplitParameterList(
              ParameterListSyntax node
            , string baseIndent
            , CommaStyle style
        )
        {
            var newList = BuildSplitSeparated(node.Parameters, baseIndent, style);
            return node
                .WithOpenParenToken(SanitizeOpen(node.OpenParenToken))
                .WithParameters(newList)
                .WithCloseParenToken(BuildClose(node.CloseParenToken, baseIndent));
        }

        private static BracketedParameterListSyntax SplitBracketedParameterList(
              BracketedParameterListSyntax node
            , string baseIndent
            , CommaStyle style
        )
        {
            var newList = BuildSplitSeparated(node.Parameters, baseIndent, style);
            return node
                .WithOpenBracketToken(SanitizeOpen(node.OpenBracketToken))
                .WithParameters(newList)
                .WithCloseBracketToken(BuildClose(node.CloseBracketToken, baseIndent));
        }

        private static ArgumentListSyntax SplitArgumentList(
              ArgumentListSyntax node
            , string baseIndent
            , CommaStyle style
        )
        {
            var newList = BuildSplitSeparated(node.Arguments, baseIndent, style);
            return node
                .WithOpenParenToken(SanitizeOpen(node.OpenParenToken))
                .WithArguments(newList)
                .WithCloseParenToken(BuildClose(node.CloseParenToken, baseIndent));
        }

        private static BracketedArgumentListSyntax SplitBracketedArgumentList(
              BracketedArgumentListSyntax node
            , string baseIndent
            , CommaStyle style
        )
        {
            var newList = BuildSplitSeparated(node.Arguments, baseIndent, style);
            return node
                .WithOpenBracketToken(SanitizeOpen(node.OpenBracketToken))
                .WithArguments(newList)
                .WithCloseBracketToken(BuildClose(node.CloseBracketToken, baseIndent));
        }

        private static AttributeArgumentListSyntax SplitAttributeArgumentList(
              AttributeArgumentListSyntax node
            , string baseIndent
            , CommaStyle style
        )
        {
            var newList = BuildSplitSeparated(node.Arguments, baseIndent, style);
            return node
                .WithOpenParenToken(SanitizeOpen(node.OpenParenToken))
                .WithArguments(newList)
                .WithCloseParenToken(BuildClose(node.CloseParenToken, baseIndent));
        }

        private static ParameterListSyntax CombineParameterList(ParameterListSyntax node)
        {
            return node
                .WithOpenParenToken(node.OpenParenToken.WithTrailingTrivia())
                .WithParameters(BuildCombineSeparated(node.Parameters))
                .WithCloseParenToken(node.CloseParenToken.WithLeadingTrivia());
        }

        private static BracketedParameterListSyntax CombineBracketedParameterList(BracketedParameterListSyntax node)
        {
            return node
                .WithOpenBracketToken(node.OpenBracketToken.WithTrailingTrivia())
                .WithParameters(BuildCombineSeparated(node.Parameters))
                .WithCloseBracketToken(node.CloseBracketToken.WithLeadingTrivia());
        }

        private static ArgumentListSyntax CombineArgumentList(ArgumentListSyntax node)
        {
            return node
                .WithOpenParenToken(node.OpenParenToken.WithTrailingTrivia())
                .WithArguments(BuildCombineSeparated(node.Arguments))
                .WithCloseParenToken(node.CloseParenToken.WithLeadingTrivia());
        }

        private static BracketedArgumentListSyntax CombineBracketedArgumentList(BracketedArgumentListSyntax node)
        {
            return node
                .WithOpenBracketToken(node.OpenBracketToken.WithTrailingTrivia())
                .WithArguments(BuildCombineSeparated(node.Arguments))
                .WithCloseBracketToken(node.CloseBracketToken.WithLeadingTrivia());
        }

        private static AttributeArgumentListSyntax CombineAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            return node
                .WithOpenParenToken(node.OpenParenToken.WithTrailingTrivia())
                .WithArguments(BuildCombineSeparated(node.Arguments))
                .WithCloseParenToken(node.CloseParenToken.WithLeadingTrivia());
        }

        private static SeparatedSyntaxList<T> BuildSplitSeparated<T>(
              SeparatedSyntaxList<T> list
            , string baseIndent
            , CommaStyle style
        )
            where T : SyntaxNode
        {
            var eol = SyntaxFactory.EndOfLine("\n");
            var itemIndent = baseIndent + INDENT_UNIT;
            var firstItemIndent = style == CommaStyle.Leading
                ? baseIndent + INDENT_UNIT + LEADING_FIRST_EXTRA
                : itemIndent;

            var nodesAndTokens = list.GetWithSeparators();
            var rebuilt = new List<SyntaxNodeOrToken>(nodesAndTokens.Count);
            var itemIndex = 0;

            for (var i = 0; i < nodesAndTokens.Count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = (T)nt.AsNode();
                    var keptLeading = KeepCommentsOnly(item.GetLeadingTrivia());
                    var keptTrailing = KeepCommentsOnly(item.GetTrailingTrivia());

                    SyntaxTriviaList newLeading;

                    if (style == CommaStyle.Leading && itemIndex > 0)
                    {
                        newLeading = keptLeading;
                    }
                    else
                    {
                        var indent = itemIndex == 0 ? firstItemIndent : itemIndent;
                        newLeading = BuildLeading(eol, indent, keptLeading);
                    }

                    item = item
                        .WithLeadingTrivia(newLeading)
                        .WithTrailingTrivia(keptTrailing);
                    rebuilt.Add(item);
                    itemIndex++;
                }
                else
                {
                    var sep = nt.AsToken();

                    if (style == CommaStyle.Leading)
                    {
                        sep = sep
                            .WithLeadingTrivia(BuildLeading(eol, itemIndent, default))
                            .WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
                    }
                    else
                    {
                        sep = sep
                            .WithLeadingTrivia()
                            .WithTrailingTrivia();
                    }

                    rebuilt.Add(sep);
                }
            }

            return SyntaxFactory.SeparatedList<T>(rebuilt);
        }

        private static SeparatedSyntaxList<T> BuildCombineSeparated<T>(SeparatedSyntaxList<T> list)
            where T : SyntaxNode
        {
            var nodesAndTokens = list.GetWithSeparators();
            var rebuilt = new List<SyntaxNodeOrToken>(nodesAndTokens.Count);

            for (var i = 0; i < nodesAndTokens.Count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = (T)nt.AsNode();
                    item = item
                        .WithLeadingTrivia()
                        .WithTrailingTrivia();
                    rebuilt.Add(item);
                }
                else
                {
                    var sep = nt.AsToken()
                        .WithLeadingTrivia()
                        .WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
                    rebuilt.Add(sep);
                }
            }

            return SyntaxFactory.SeparatedList<T>(rebuilt);
        }

        private static SyntaxToken SanitizeOpen(SyntaxToken open)
        {
            return open.WithTrailingTrivia();
        }

        private static SyntaxToken BuildClose(SyntaxToken close, string baseIndent)
        {
            var eol = SyntaxFactory.EndOfLine("\n");

            if (baseIndent.Length == 0)
            {
                return close.WithLeadingTrivia(eol);
            }

            var ws = SyntaxFactory.Whitespace(baseIndent);
            return close.WithLeadingTrivia(eol, ws);
        }

        private static SyntaxTriviaList BuildLeading(
              SyntaxTrivia eol
            , string indent
            , SyntaxTriviaList comments
        )
        {
            var ws = SyntaxFactory.Whitespace(indent);

            if (comments.Count == 0)
            {
                return SyntaxFactory.TriviaList(eol, ws);
            }

            var list = new List<SyntaxTrivia>(2 + comments.Count * 3);

            for (var i = 0; i < comments.Count; i++)
            {
                list.Add(eol);
                list.Add(ws);
                list.Add(comments[i]);
            }

            list.Add(eol);
            list.Add(ws);
            return SyntaxFactory.TriviaList(list);
        }

        private static SyntaxTriviaList KeepCommentsOnly(SyntaxTriviaList trivia)
        {
            List<SyntaxTrivia> kept = null;

            for (var i = 0; i < trivia.Count; i++)
            {
                var t = trivia[i];

                if (IsCommentTrivia(t) == false)
                {
                    continue;
                }

                kept ??= new List<SyntaxTrivia>();
                kept.Add(t);
            }

            if (kept is null)
            {
                return default;
            }

            return SyntaxFactory.TriviaList(kept);
        }

        private static bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            var kind = trivia.Kind();
            return kind == SyntaxKind.SingleLineCommentTrivia
                || kind == SyntaxKind.MultiLineCommentTrivia
                || kind == SyntaxKind.SingleLineDocumentationCommentTrivia
                || kind == SyntaxKind.MultiLineDocumentationCommentTrivia;
        }
    }
}
