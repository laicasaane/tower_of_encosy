using System;

namespace EncosyTower.Collections
{
    public interface IAsSpan<T>
    {
        Span<T> AsSpan();
    }

    public interface IAsReadOnlySpan<T>
    {
        ReadOnlySpan<T> AsReadOnlySpan();
    }
}
