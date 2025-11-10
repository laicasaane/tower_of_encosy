#if UNITY_COLLECTIONS

using System;
using EncosyTower.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace EncosyTower.Jobs
{
    [BurstCompile]
    public partial struct TrySetCapacityArrayMapNativeJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        public int capacity;
        public ArrayMapNative<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.IncreaseCapacityTo(capacity);
        }
    }

    [BurstCompile]
    public partial struct ClearArrayMapNativeJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [WriteOnly] public ArrayMapNative<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.Clear();
        }
    }

    [BurstCompile]
    public partial struct ClearSharedArrayMapNativeJob<TKey, TValue> : IJob
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [WriteOnly] public SharedArrayMapNative<TKey, TValue> map;

        [BurstCompile]
        public void Execute()
        {
            map.Clear();
        }
    }
}

#endif
