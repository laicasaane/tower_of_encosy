using System;

namespace Module.Core
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
