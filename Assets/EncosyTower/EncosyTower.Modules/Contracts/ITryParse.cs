using System;

namespace EncosyTower.Modules
{
    public interface ITryParse<T>
    {
        bool TryParse(string str, out T result);
    }

    public interface ITryParseSpan<T>
    {
        bool TryParse(ReadOnlySpan<char> str, out T result);
    }
}
