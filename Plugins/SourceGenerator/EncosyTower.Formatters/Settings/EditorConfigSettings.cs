using System;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Formatters.Settings
{
    internal static class EditorConfigSettings
    {
        private const string KEY_COMMA_STYLE = "encosy_formatter_comma_style";
        private const string VALUE_TRAILING = "trailing";

        public static CommaStyle GetCommaStyle(Document document, SyntaxTree tree)
        {
            var opts = document.Project.AnalyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(tree);

            if (opts.TryGetValue(KEY_COMMA_STYLE, out var raw)
                && string.Equals(raw, VALUE_TRAILING, StringComparison.OrdinalIgnoreCase)
            )
            {
                return CommaStyle.Trailing;
            }

            return CommaStyle.Leading;
        }
    }
}
