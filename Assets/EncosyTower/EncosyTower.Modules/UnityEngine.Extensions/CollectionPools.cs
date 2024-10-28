using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using UnityEngine.Pool;

namespace EncosyTower.Modules.Pooling
{
    public sealed class FasterListPool<T> : CollectionPool<FasterList<T>, T> { }

    public sealed class ArrayMapPool<TKey, TValue>
    {
        internal static readonly ObjectPool<ArrayMap<TKey, TValue>> s_pool
            = new(static () => new(), null, static x => x.Clear());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayMap<TKey, TValue> Get()
            => s_pool.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObject<ArrayMap<TKey, TValue>> Get(out ArrayMap<TKey, TValue> value)
            => s_pool.Get(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(ArrayMap<TKey, TValue> toRelease)
            => s_pool.Release(toRelease);
    }
}