using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using UnityEngine.Pool;

namespace EncosyTower.Pooling
{
    public sealed class QueuePool<T>
    {
        internal static readonly ObjectPool<Queue<T>> s_pool
            = new(static () => new(), null, static x => x.Clear());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Queue<T> Get()
            => s_pool.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObject<Queue<T>> Get(out Queue<T> value)
            => s_pool.Get(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(Queue<T> toRelease)
            => s_pool.Release(toRelease);
    }

    public sealed class StackPool<T>
    {
        internal static readonly ObjectPool<Stack<T>> s_pool
            = new(static () => new(), null, static x => x.Clear());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stack<T> Get()
            => s_pool.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObject<Stack<T>> Get(out Stack<T> value)
            => s_pool.Get(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(Stack<T> toRelease)
            => s_pool.Release(toRelease);
    }

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
