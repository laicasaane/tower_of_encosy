using System.Text;

namespace EncosyTower.Modules.SourceGen
{
    public static class StringExtensions
    {
        public static string ToValidIdentifier(this string value)
            => value
                .Replace('.', '_')
                .Replace("-", "__")
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                .Replace("[]", "Array")
                ;

        public static int GetByteCount(this string value)
        {
            if (value == null)
                return 0;

            return Encoding.UTF8.GetByteCount(value);
        }
    }
}
