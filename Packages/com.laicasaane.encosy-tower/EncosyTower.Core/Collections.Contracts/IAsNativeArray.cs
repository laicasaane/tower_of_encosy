using Unity.Collections;

namespace EncosyTower.Collections
{
    public interface IAsNativeArray<T> where T : struct
    {
        NativeArray<T> AsNativeArray();
    }

    public interface IAsNativeArrayReadOnly<T> where T : struct
    {
        NativeArray<T>.ReadOnly AsNativeArrayReadOnly();
    }
}
