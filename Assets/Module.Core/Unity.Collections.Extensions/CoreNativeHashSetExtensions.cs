#if UNITY_COLLECTIONS

using System;
using Unity.Collections;

namespace Module.Core.Collections
{
    public static class CoreNativeHashSetExtensions
    {
        public static ref NativeHashSet<T> NewOrClear<T>(
              ref this NativeHashSet<T> set
            , int capacity
            , AllocatorManager.AllocatorHandle allocator
        )
            where T : unmanaged, IEquatable<T>
        {
            if (set.IsCreated)
            {
                set.Clear();
            }
            else
            {
                set = new(capacity, allocator);
            }

            return ref set;
        }

        public static void DisposeUnset<T>(ref this NativeHashSet<T> set)
            where T : unmanaged, IEquatable<T>
        {
            if (set.IsCreated)
            {
                set.Dispose();
            }

            set = default;
        }
    }
}

#endif
