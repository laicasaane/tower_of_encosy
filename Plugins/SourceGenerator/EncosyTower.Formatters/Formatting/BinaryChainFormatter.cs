using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EncosyTower.Formatters.Settings;

namespace EncosyTower.Formatters.Formatting
{
    internal static class BinaryChainFormatter
    {
        private const string INDENT_UNIT = "    ";

        public static ExpressionSyntax SplitBinary(
              BinaryExpressionSyntax root
            , List<ExpressionSyntax> items
            , List<SyntaxToken> ops
            , string baseIndent
            , SeparatorStyle style
        )
        {
            var indent = baseIndent + INDENT_UNIT;
            ApplySplitTrivia(items, ops, indent, style);
            return Rebuild(root.Kind(), items, ops);
        }

        public static PatternSyntax SplitPattern(
              BinaryPatternSyntax root
            , List<PatternSyntax> items
            , List<SyntaxToken> ops
            , string baseIndent
            , SeparatorStyle style
        )
        {
            var indent = baseIndent + INDENT_UNIT;
            ApplySplitTriviaPattern(items, ops, indent, style);
            return RebuildPattern( items, ops);
        }

        public static ExpressionSyntax CombineBinary(
              BinaryExpressionSyntax root
            , List<ExpressionSyntax> items
            , List<SyntaxToken> ops
        )
        {
            ApplyCombineTrivia(items, ops);
            return Rebuild(root.Kind(), items, ops);
        }

        public static PatternSyntax CombinePattern(
              BinaryPatternSyntax root
            , List<PatternSyntax> items
            , List<SyntaxToken> ops
        )
        {
            ApplyCombineTriviaPattern(items, ops);
            return RebuildPattern(items, ops);
        }

        public static bool ChainHasComment(List<ExpressionSyntax> items, List<SyntaxToken> ops)
        {
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                if (TriviaUtil.TriviaListHasComment(items[i].GetLeadingTrivia())
                    || TriviaUtil.TriviaListHasComment(items[i].GetTrailingTrivia())
                )
                {
                    return true;
                }
            }

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                if (TriviaUtil.TriviaListHasComment(ops[i].LeadingTrivia)
                    || TriviaUtil.TriviaListHasComment(ops[i].TrailingTrivia)
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ChainHasCommentPattern(List<PatternSyntax> items, List<SyntaxToken> ops)
        {
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                if (TriviaUtil.TriviaListHasComment(items[i].GetLeadingTrivia())
                    || TriviaUtil.TriviaListHasComment(items[i].GetTrailingTrivia())
                )
                {
                    return true;
                }
            }

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                if (TriviaUtil.TriviaListHasComment(ops[i].LeadingTrivia)
                    || TriviaUtil.TriviaListHasComment(ops[i].TrailingTrivia)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplySplitTrivia(
              List<ExpressionSyntax> items
            , List<SyntaxToken> ops
            , string indent
            , SeparatorStyle style
        )
        {
            var eol = TriviaUtil.Eol();
            var ws = TriviaUtil.Indent(indent);
            var space = TriviaUtil.Space();
            var firstLeading = items[0].GetLeadingTrivia();
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                if (style == SeparatorStyle.Leading || i == 0)
                {
                    items[i] = items[i].WithLeadingTrivia().WithTrailingTrivia();
                }
                else
                {
                    items[i] = items[i]
                        .WithLeadingTrivia(eol, ws)
                        .WithTrailingTrivia();
                }
            }

            items[0] = items[0].WithLeadingTrivia(firstLeading);

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                if (style == SeparatorStyle.Leading)
                {
                    ops[i] = ops[i]
                        .WithLeadingTrivia(eol, ws)
                        .WithTrailingTrivia(space);
                }
                else
                {
                    ops[i] = ops[i]
                        .WithLeadingTrivia(space)
                        .WithTrailingTrivia();
                }
            }
        }

        private static void ApplySplitTriviaPattern(
              List<PatternSyntax> items
            , List<SyntaxToken> ops
            , string indent
            , SeparatorStyle style
        )
        {
            var eol = TriviaUtil.Eol();
            var ws = TriviaUtil.Indent(indent);
            var space = TriviaUtil.Space();
            var firstLeading = items[0].GetLeadingTrivia();
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                if (style == SeparatorStyle.Leading || i == 0)
                {
                    items[i] = items[i].WithLeadingTrivia().WithTrailingTrivia();
                }
                else
                {
                    items[i] = items[i]
                        .WithLeadingTrivia(eol, ws)
                        .WithTrailingTrivia();
                }
            }

            items[0] = items[0].WithLeadingTrivia(firstLeading);

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                if (style == SeparatorStyle.Leading)
                {
                    ops[i] = ops[i]
                        .WithLeadingTrivia(eol, ws)
                        .WithTrailingTrivia(space);
                }
                else
                {
                    ops[i] = ops[i]
                        .WithLeadingTrivia(space)
                        .WithTrailingTrivia();
                }
            }
        }

        private static void ApplyCombineTrivia(List<ExpressionSyntax> items, List<SyntaxToken> ops)
        {
            var firstLeading = items[0].GetLeadingTrivia();
            var lastTrailing = items[items.Count - 1].GetTrailingTrivia();
            var space = TriviaUtil.Space();
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                items[i] = items[i].WithLeadingTrivia().WithTrailingTrivia();
            }

            items[0] = items[0].WithLeadingTrivia(firstLeading);
            items[itemCount - 1] = items[itemCount - 1].WithTrailingTrivia(lastTrailing);

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                ops[i] = ops[i]
                    .WithLeadingTrivia(space)
                    .WithTrailingTrivia(space);
            }
        }

        private static void ApplyCombineTriviaPattern(List<PatternSyntax> items, List<SyntaxToken> ops)
        {
            var firstLeading = items[0].GetLeadingTrivia();
            var lastTrailing = items[items.Count - 1].GetTrailingTrivia();
            var space = TriviaUtil.Space();
            var itemCount = items.Count;

            for (var i = 0; i < itemCount; i++)
            {
                items[i] = items[i].WithLeadingTrivia().WithTrailingTrivia();
            }

            items[0] = items[0].WithLeadingTrivia(firstLeading);
            items[itemCount - 1] = items[itemCount - 1].WithTrailingTrivia(lastTrailing);

            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                ops[i] = ops[i]
                    .WithLeadingTrivia(space)
                    .WithTrailingTrivia(space);
            }
        }

        private static ExpressionSyntax Rebuild(
              SyntaxKind kind
            , List<ExpressionSyntax> items
            , List<SyntaxToken> ops
        )
        {
            ExpressionSyntax expr = items[0];
            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                expr = SyntaxFactory.BinaryExpression(kind, expr, ops[i], items[i + 1]);
            }

            return expr;
        }

        private static PatternSyntax RebuildPattern(
              List<PatternSyntax> items
            , List<SyntaxToken> ops
        )
        {
            PatternSyntax pat = items[0];
            var opCount = ops.Count;

            for (var i = 0; i < opCount; i++)
            {
                var opKind = ops[i].IsKind(SyntaxKind.OrKeyword)
                    ? SyntaxKind.OrPattern
                    : SyntaxKind.AndPattern;
                pat = SyntaxFactory.BinaryPattern(opKind, pat, ops[i], items[i + 1]);
            }

            return pat;
        }
    }
}
