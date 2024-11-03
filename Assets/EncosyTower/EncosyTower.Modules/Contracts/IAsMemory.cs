using System;

namespace EncosyTower.Modules
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
