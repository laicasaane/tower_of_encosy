using System.Collections.Concurrent;
using System.Text;

namespace EncosyTower.SourceGen
{
    public static class StringExtensions
    {
        private static readonly ConcurrentQueue<StringBuilder> s_sbPool = new();

        public static string EscapeStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = RentSb();
            var result = EscapeStringLiteral(value, sb);
            ReturnSb(sb);
            return result;
        }

        public static string EscapeStringLiteral(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : EscapeStringLiteral_Internal(value, sb);

        public static string ToValidIdentifier(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = RentSb();
            var result = ToValidIdentifier(value, sb);
            ReturnSb(sb);
            return result;
        }

        public static string ToValidIdentifier(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : ToValidIdentifier_Internal(value, sb);

        public static string ToFileName(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sb = RentSb();
            var result = ToFileName(value, sb);
            ReturnSb(sb);
            return result;
        }

        public static string ToFileName(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : ToFileName_Internal(value, sb);

        public static int GetByteCount(this string value)
            => value == null ? 0 : Encoding.UTF8.GetByteCount(value);

        public static StringBuilder RentSb()
            => s_sbPool.TryDequeue(out var sb) ? sb : new StringBuilder();

        public static void ReturnSb(StringBuilder sb)
        {
            sb.Clear();
            s_sbPool.Enqueue(sb);
        }

        private static string EscapeStringLiteral_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            return sb.ToString();
        }

        private static string ToValidIdentifier_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("global::", "")
                .Replace(' ', '_')
                .Replace(':', '_')
                .Replace('.', '_')
                .Replace("-", "__")
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                .Replace("[]", "Array")
                ;

            return sb.ToString();
        }

        private static string ToFileName_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("global::", "")
                .Replace('\\', '-')
                .Replace('/', '-')
                .Replace(':', '-')
                .Replace('*', '-')
                .Replace('?', '-')
                .Replace('|', '-')
                .Replace('\"', '\'')
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                ;

            return sb.ToString();
        }
    }
}
