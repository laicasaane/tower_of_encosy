using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.Formatters.Formatting
{
    internal static class TriviaUtil
    {
        public static SyntaxTrivia Eol()
        {
            return SyntaxFactory.EndOfLine("\n");
        }

        public static SyntaxTrivia Indent(string indent)
        {
            return SyntaxFactory.Whitespace(indent);
        }

        public static SyntaxTrivia Space()
        {
            return SyntaxFactory.Whitespace(" ");
        }

        public static bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            var kind = trivia.Kind();
            return kind == SyntaxKind.SingleLineCommentTrivia
                || kind == SyntaxKind.MultiLineCommentTrivia
                || kind == SyntaxKind.SingleLineDocumentationCommentTrivia
                || kind == SyntaxKind.MultiLineDocumentationCommentTrivia;
        }

        public static bool TriviaListHasComment(SyntaxTriviaList trivia)
        {
            var count = trivia.Count;

            for (var i = 0; i < count; i++)
            {
                if (IsCommentTrivia(trivia[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static SyntaxTriviaList KeepCommentsOnly(SyntaxTriviaList trivia)
        {
            List<SyntaxTrivia> kept = null;
            var count = trivia.Count;

            for (var i = 0; i < count; i++)
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
    }
}
