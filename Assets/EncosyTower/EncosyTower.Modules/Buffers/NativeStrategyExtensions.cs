#if UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections.Unsafe;

namespace EncosyTower.Modules.Buffers
{
    public static class NativeStrategyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this ref NativeStrategy<T> self, int index, int count)
            where T : struct
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index)
                return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var array = self._realBuffer.ToNativeArray();
            array.MemoryMove(index + 1, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this ref NativeStrategy<T> self, int index, int count)
            where T : struct
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index)
                return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var array = self._realBuffer.ToNativeArray();
            array.MemoryMove(index, index + 1, count - index);
        }
    }
}

#endif
