using System;

namespace EncosyTower.Modules.Data.Authoring
{
    public static class SheetUtility
    {
        public static bool ValidateSheetName(string name, bool allowComments = false, bool allowWhiteSpaces = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            if (allowWhiteSpaces == false && name.Contains(' ', StringComparison.Ordinal))
                return false;

            if (allowComments)
            {
                return true;
            }

            return name.StartsWith('$') == false
                && name.StartsWith('<') == false
                && name.EndsWith('>') == false
                ;
        }

        public static string ToFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = name.Trim();

            if (name.StartsWith('<') || name.EndsWith('>'))
            {
                return $"${name.Replace("<", "", StringComparison.Ordinal).Replace(">", "", StringComparison.Ordinal)}";
            }

            return name;
        }

        public static string ToFileName(string name, int index)
        {
            if (string.IsNullOrWhiteSpace(name))
                return $"${index}";

            name = name.Trim();

            if (name.StartsWith('<') || name.EndsWith('>'))
            {
                return $"${name.Replace("<", "", StringComparison.Ordinal).Replace(">", "", StringComparison.Ordinal)}";
            }

            return name;
        }
    }
}
