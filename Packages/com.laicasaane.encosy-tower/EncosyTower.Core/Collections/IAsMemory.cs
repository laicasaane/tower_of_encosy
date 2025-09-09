using System;

namespace EncosyTower.Collections
{
    public interface IAsMemory<T>
    {
        Memory<T> AsMemory();
    }

    public interface IAsReadOnlyMemory<T>
    {
        ReadOnlyMemory<T> AsReadOnlyMemory();
    }
}
