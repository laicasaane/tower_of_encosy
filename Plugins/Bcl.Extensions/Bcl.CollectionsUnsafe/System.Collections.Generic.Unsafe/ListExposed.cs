using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic.Unsafe;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct ListExposed<T>([NotNull] List<T> list)
{
    public readonly List<T> List = list;

    public ref T?[] Item
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref List._items;
    }

    public ref int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref List._size;
    }

    public ref int Version
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref List._version;
    }
}
