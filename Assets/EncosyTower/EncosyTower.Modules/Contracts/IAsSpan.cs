using System;

namespace EncosyTower.Modules
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
