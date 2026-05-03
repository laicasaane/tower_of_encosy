using System.Collections.Generic;
using EncosyTower.Formatters.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Formatting
{
    internal static class ConstraintsFormatter
    {
        private const string INDENT_UNIT = "    ";

        public static TypeParameterConstraintClauseSyntax Split(
              TypeParameterConstraintClauseSyntax clause
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
                newColon = clause.ColonToken
                    .WithLeadingTrivia(space)
                    .WithTrailingTrivia(space);
            }
            else
            {
                newColon = clause.ColonToken
                    .WithLeadingTrivia(space)
                    .WithTrailingTrivia();
            }

            var nodesAndTokens = clause.Constraints.GetWithSeparators();
            var rebuilt = new List<SyntaxNodeOrToken>(nodesAndTokens.Count);

            for (var i = 0; i < nodesAndTokens.Count; i++)
            {
                var nt = nodesAndTokens[i];

                if (nt.IsNode)
                {
                    var item = (TypeParameterConstraintSyntax)nt.AsNode();

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

            var newName = clause.Name.WithTrailingTrivia();

            return clause
                .WithName(newName)
                .WithColonToken(newColon)
                .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(rebuilt));
        }

        public static TypeParameterConstraintClauseSyntax Combine(TypeParameterConstraintClauseSyntax clause)
        {
            var space = TriviaUtil.Space();

            var newColon = clause.ColonToken
                .WithLeadingTrivia(space)
                .WithTrailingTrivia(space);

            var nodesAndTokens = clause.Constraints.GetWithSeparators();
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

            var newName = clause.Name.WithTrailingTrivia();

            return clause
                .WithName(newName)
                .WithColonToken(newColon)
                .WithConstraints(SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>(rebuilt));
        }

        public static bool HasComment(TypeParameterConstraintClauseSyntax clause)
        {
            if (TriviaUtil.TriviaListHasComment(clause.ColonToken.LeadingTrivia)
                || TriviaUtil.TriviaListHasComment(clause.ColonToken.TrailingTrivia)
            )
            {
                return true;
            }

            var nodesAndTokens = clause.Constraints.GetWithSeparators();
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

        public static bool IsSingleLine(TypeParameterConstraintClauseSyntax clause)
        {
            var tree = clause.SyntaxTree;

            if (tree is null)
            {
                return true;
            }

            var firstLine = tree.GetLineSpan(clause.ColonToken.Span).StartLinePosition.Line;
            var lastLine = tree.GetLineSpan(clause.GetLastToken().Span).StartLinePosition.Line;
            return firstLine == lastLine;
        }
    }
}
