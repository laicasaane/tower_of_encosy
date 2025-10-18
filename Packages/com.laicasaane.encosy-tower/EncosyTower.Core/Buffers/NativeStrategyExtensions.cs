#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using EncosyTower.Collections.Unsafe;
using EncosyTower.Debugging;

namespace EncosyTower.Buffers
{
    public static class NativeStrategyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this NativeStrategy<T> self, int index, int count)
            where T : unmanaged
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index)
                return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var array = self._realBuffer.ToNativeArray();
            array.MemoryCopyUnsafe(index + 1, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this NativeStrategy<T> self, int index, int count)
            where T : unmanaged
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index)
                return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var array = self._realBuffer.ToNativeArray();
            array.MemoryCopyUnsafe(index, index + 1, count - index);
        }
    }
}

#endif
