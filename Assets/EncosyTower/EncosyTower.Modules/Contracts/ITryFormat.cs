using System;

namespace EncosyTower.Modules
{
    public interface ITryFormat
    {
        bool TryFormat(
              Span<char> destination
            , out int charsWritten
            , ReadOnlySpan<char> format = default
            , IFormatProvider provider = null
        );
    }
}
