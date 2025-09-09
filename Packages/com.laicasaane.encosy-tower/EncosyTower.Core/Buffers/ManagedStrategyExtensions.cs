using System;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Buffers
{
    public static class ManagedStrategyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftLeft<T>(this ManagedStrategy<T> self, int index, int count)
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index) return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var managedArray = self._realBuffer.ToManagedArray();
            Array.Copy(managedArray, index + 1, managedArray, index, count - index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShiftRight<T>(this ManagedStrategy<T> self, int index, int count)
        {
            Checks.IsTrue(index < self.Capacity, "Out of bounds index");
            Checks.IsTrue(count < self.Capacity, "Out of bounds count");

            if (count == index) return;

            Checks.IsTrue(count > index, "Count is lesser than index");

            var managedArray = self._realBuffer.ToManagedArray();
            Array.Copy(managedArray, index, managedArray, index + 1, count - index);
        }
    }
}
