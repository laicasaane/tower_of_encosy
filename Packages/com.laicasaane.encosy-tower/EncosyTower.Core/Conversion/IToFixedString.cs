#if UNITY_COLLECTIONS

using Unity.Collections;

namespace EncosyTower.Conversion
{
    public interface IToFixedString<out T>
        where T : unmanaged, INativeList<byte>, IUTF8Bytes
    {
        T ToFixedString();
    }

    public interface IToDisplayFixedString<out T>
        where T : unmanaged, INativeList<byte>, IUTF8Bytes
    {
        T ToDisplayFixedString();
    }

    public interface IToFixedString
    {
        T ToFixedString<T>()
            where T : unmanaged, INativeList<byte>, IUTF8Bytes;
    }

    public interface IToDisplayFixedString
    {
        T ToDisplayFixedString<T>()
            where T : unmanaged, INativeList<byte>, IUTF8Bytes;
    }
}

#endif
