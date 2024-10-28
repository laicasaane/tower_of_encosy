#if UNITY_COLLECTIONS

using System;
using Unity.Collections;

namespace EncosyTower.Modules
{
    public interface IToFixedString<T>
        where T : unmanaged, INativeList<byte>, IIndexable<byte>, IUTF8Bytes
                , IComparable<string>, IEquatable<string>
                , IComparable<T>, IEquatable<T>
    {
        T ToFixedString();
    }

    public interface IToDisplayFixedString<T>
        where T : unmanaged, INativeList<byte>, IIndexable<byte>, IUTF8Bytes
                , IComparable<string>, IEquatable<string>
                , IComparable<T>, IEquatable<T>
    {
        T ToDisplayFixedString();
    }
}

#endif