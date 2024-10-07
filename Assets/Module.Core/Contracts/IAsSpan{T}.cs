using System;

namespace Module.Core
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
