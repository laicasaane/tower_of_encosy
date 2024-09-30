using System;

namespace Module.Core
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
