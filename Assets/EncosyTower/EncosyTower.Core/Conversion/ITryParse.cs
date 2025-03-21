using System;

namespace EncosyTower.Conversion
{
    public interface ITryParse<T>
    {
        bool TryParse(string str, out T result, bool ignoreCase, bool allowMatchingMetadataAttribute);
    }

    public interface ITryParseSpan<T>
    {
        bool TryParse(ReadOnlySpan<char> str, out T result, bool ignoreCase, bool allowMatchingMetadataAttribute);
    }
}
