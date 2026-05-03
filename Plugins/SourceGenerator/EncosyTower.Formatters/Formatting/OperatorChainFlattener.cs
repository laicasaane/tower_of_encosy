using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Formatters.Formatting
{
    internal static class OperatorChainFlattener
    {
        public static BinaryExpressionSyntax FindBinaryChainRoot(BinaryExpressionSyntax node)
        {
            var kind = node.Kind();
            var current = node;

            while (current.Parent is BinaryExpressionSyntax parent && parent.Kind() == kind)
            {
                current = parent;
            }

            return current;
        }

        public static BinaryPatternSyntax FindBinaryPatternChainRoot(BinaryPatternSyntax node)
        {
            var current = node;

            while (current.Parent is BinaryPatternSyntax parent)
            {
                current = parent;
            }

            return current;
        }

        public static bool TryFlatten(
              BinaryExpressionSyntax root
            , out List<ExpressionSyntax> items
            , out List<SyntaxToken> ops
        )
        {
            items = new List<ExpressionSyntax>();
            ops = new List<SyntaxToken>();
            FlattenBinary(root, root.Kind(), items, ops);

            if (items.Count < 2)
            {
                return false;
            }

            return true;
        }

        public static bool TryFlatten(
              BinaryPatternSyntax root
            , out List<PatternSyntax> items
            , out List<SyntaxToken> ops
        )
        {
            items = new List<PatternSyntax>();
            ops = new List<SyntaxToken>();
            FlattenBinaryPattern(root, root.Kind(), items, ops);

            if (items.Count < 2)
            {
                return false;
            }

            return true;
        }

        private static void FlattenBinary(
              BinaryExpressionSyntax node
            , SyntaxKind kind
            , List<ExpressionSyntax> items
            , List<SyntaxToken> ops
        )
        {
            if (node.Left is BinaryExpressionSyntax leftBin && leftBin.Kind() == kind)
            {
                FlattenBinary(leftBin, kind, items, ops);
            }
            else
            {
                items.Add(node.Left);
            }

            ops.Add(node.OperatorToken);

            if (node.Right is BinaryExpressionSyntax rightBin && rightBin.Kind() == kind)
            {
                FlattenBinary(rightBin, kind, items, ops);
            }
            else
            {
                items.Add(node.Right);
            }
        }

        private static void FlattenBinaryPattern(
              BinaryPatternSyntax node
            , SyntaxKind kind
            , List<PatternSyntax> items
            , List<SyntaxToken> ops
        )
        {
            if (node.Left is BinaryPatternSyntax leftBin)
            {
                FlattenBinaryPattern(leftBin, leftBin.Kind(), items, ops);
            }
            else
            {
                items.Add(node.Left);
            }

            ops.Add(node.OperatorToken);

            if (node.Right is BinaryPatternSyntax rightBin && rightBin.Kind() == kind)
            {
                FlattenBinaryPattern(rightBin, kind, items, ops);
            }
            else
            {
                items.Add(node.Right);
            }
        }

        public static bool IsSingleLine(SyntaxNode node)
        {
            var tree = node.SyntaxTree;

            if (tree is null)
            {
                return true;
            }

            var firstLine = tree.GetLineSpan(node.GetFirstToken().Span).StartLinePosition.Line;
            var lastLine = tree.GetLineSpan(node.GetLastToken().Span).StartLinePosition.Line;
            return firstLine == lastLine;
        }
    }
}
