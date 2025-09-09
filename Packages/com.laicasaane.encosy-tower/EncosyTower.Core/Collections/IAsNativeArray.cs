using Unity.Collections;

namespace EncosyTower.Collections
{
    public interface IAsNativeArray<T> where T : struct
    {
        NativeArray<T> AsNativeArray();
    }

    public interface IAsNativeSlice<T> where T : struct
    {
        NativeSlice<T> AsNativeSlice();
    }
}
