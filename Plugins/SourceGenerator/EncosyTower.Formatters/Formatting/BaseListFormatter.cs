using System.Collections.Generic;
using EncosyTower.Formatters.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Formatting
{
    internal static class BaseListFormatter
    {
        private const string INDENT_UNIT = "    ";

        public static BaseListSyntax Split(
              BaseListSyntax baseList
            , string baseIndent
            , SeparatorStyle style
        )
        {
            var indent = baseIndent + INDENT_UNIT;
            var eol = TriviaUtil.Eol();
            var ws = TriviaUtil.Indent(indent);
            var space = TriviaUtil.Space();

            SyntaxToken newColon;

            if (style == SeparatorStyle.Leading)
            {
                newColon = baseList.ColonToken
                    .WithLeadingTrivia(eol, ws)
                    .WithTrailingTrivia(space);
            }
            else
            {
                newColon = baseList.ColonToken
                    .WithLeadingTrivia(space)
                    .WithTrailingTrivia();
            }

            var nodesAndTokens = baseList.Types.GetWithSeparators();
            var rebuilt = new List<SyntaxNodeOrToken>(nodesAndTokens.Count);

            for (var i = 0; i < nodesAndTokens.Count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = (BaseTypeSyntax)nt.AsNode();

                    if (style == SeparatorStyle.Leading)
                    {
                        item = item.WithLeadingTrivia().WithTrailingTrivia();
                    }
                    else
                    {
                        item = item
                            .WithLeadingTrivia(eol, ws)
                            .WithTrailingTrivia();
                    }

                    rebuilt.Add(item);
                }
                else
                {
                    var sep = nt.AsToken();

                    if (style == SeparatorStyle.Leading)
                    {
                        sep = sep
                            .WithLeadingTrivia(eol, ws)
                            .WithTrailingTrivia(space);
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

            return baseList
                .WithColonToken(newColon)
                .WithTypes(SyntaxFactory.SeparatedList<BaseTypeSyntax>(rebuilt));
        }

        public static BaseListSyntax Combine(BaseListSyntax baseList)
        {
            var space = TriviaUtil.Space();

            var newColon = baseList.ColonToken
                .WithLeadingTrivia(space)
                .WithTrailingTrivia(space);

            var nodesAndTokens = baseList.Types.GetWithSeparators();
            var rebuilt = new List<SyntaxNodeOrToken>(nodesAndTokens.Count);

            for (var i = 0; i < nodesAndTokens.Count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = nt.AsNode()
                        .WithLeadingTrivia()
                        .WithTrailingTrivia();
                    rebuilt.Add(item);
                }
                else
                {
                    var sep = nt.AsToken()
                        .WithLeadingTrivia()
                        .WithTrailingTrivia(space);
                    rebuilt.Add(sep);
                }
            }

            return baseList
                .WithColonToken(newColon)
                .WithTypes(SyntaxFactory.SeparatedList<BaseTypeSyntax>(rebuilt));
        }

        public static bool HasComment(BaseListSyntax baseList)
        {
            if (TriviaUtil.TriviaListHasComment(baseList.ColonToken.LeadingTrivia)
                || TriviaUtil.TriviaListHasComment(baseList.ColonToken.TrailingTrivia)
            )
            {
                return true;
            }

            var nodesAndTokens = baseList.Types.GetWithSeparators();
            var count = nodesAndTokens.Count;

            for (var i = 0; i < count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = nt.AsNode();

                    if (TriviaUtil.TriviaListHasComment(item.GetLeadingTrivia())
                        || TriviaUtil.TriviaListHasComment(item.GetTrailingTrivia())
                    )
                    {
                        return true;
                    }
                }
                else
                {
                    var tok = nt.AsToken();

                    if (TriviaUtil.TriviaListHasComment(tok.LeadingTrivia)
                        || TriviaUtil.TriviaListHasComment(tok.TrailingTrivia)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsSingleLine(BaseListSyntax baseList)
        {
            var tree = baseList.SyntaxTree;

            if (tree is null)
            {
                return true;
            }

            var firstLine = tree.GetLineSpan(baseList.ColonToken.Span).StartLinePosition.Line;
            var lastLine = tree.GetLineSpan(baseList.GetLastToken().Span).StartLinePosition.Line;
            return firstLine == lastLine;
        }
    }
}
