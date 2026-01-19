using System.Collections.Concurrent;
using System.Text;

namespace EncosyTower.SourceGen
{
    public static class StringExtensions
    {
        private static readonly ConcurrentQueue<StringBuilder> s_sbPool = new();

        public static string ToValidIdentifier(this string value)
            => value
                .Replace('.', '_')
                .Replace("-", "__")
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                .Replace("[]", "Array")
                ;

        public static string ToFileName(this string value)
        {
            var sb = RentSb();
            var result = ToFileName(value, sb);
            ReturnSb(sb);
            return result;
        }

        public static string ToFileName(this string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
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

        public static int GetByteCount(this string value)
        {
            if (value == null)
                return 0;

            return Encoding.UTF8.GetByteCount(value);
        }

        private static StringBuilder RentSb()
        {
            if (s_sbPool.TryDequeue(out var sb))
            {
                return sb;
            }

            return new StringBuilder();
        }

        private static void ReturnSb(StringBuilder sb)
        {
            sb.Clear();
            s_sbPool.Enqueue(sb);
        }
    }
}
