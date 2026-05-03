using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Formatters.Settings
{
    internal static class EditorConfigSettings
    {
        private const string KEY_COMMA_STYLE = "encosy_formatter_comma_style";
        private const string KEY_OPERATOR_STYLE = "encosy_formatter_operator_style";
        private const string KEY_INHERITANCE_STYLE = "encosy_formatter_inheritance_style";
        private const string VALUE_TRAILING = "trailing";

        public static SeparatorStyle GetCommaStyle(Document document, SyntaxTree tree)
            => Read(document, tree, KEY_COMMA_STYLE);

        public static SeparatorStyle GetOperatorStyle(Document document, SyntaxTree tree)
            => Read(document, tree, KEY_OPERATOR_STYLE);

        public static SeparatorStyle GetInheritanceStyle(Document document, SyntaxTree tree)
            => Read(document, tree, KEY_INHERITANCE_STYLE);

        private static SeparatorStyle Read(Document document, SyntaxTree tree, string key)
        {
            var opts = document.Project.AnalyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(tree);

            if (opts.TryGetValue(key, out var raw)
                && string.Equals(raw, VALUE_TRAILING, StringComparison.OrdinalIgnoreCase)
            )
            {
                return SeparatorStyle.Trailing;
            }

            return SeparatorStyle.Leading;
        }
    }
}
