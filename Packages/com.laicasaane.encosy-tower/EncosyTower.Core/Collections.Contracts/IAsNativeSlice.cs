using EncosyTower.Buffers;
using Unity.Collections;

namespace EncosyTower.Collections
{
    public interface IAsNativeSlice<T> where T : struct
    {
        NativeSlice<T> AsNativeSlice();
    }

    public interface IAsNativeSliceReadOnly<T> where T : struct
    {
        NativeSliceReadOnly<T> AsNativeSliceReadOnly();
    }
}
