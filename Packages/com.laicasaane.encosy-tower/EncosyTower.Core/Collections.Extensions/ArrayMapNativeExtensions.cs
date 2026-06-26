#if UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Collections.Extensions
{
    public static class ArrayMapNativeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TValue> GetValues<TKey, TValue>(this in ArrayMapNative<TKey, TValue> self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values.AsRealBuffer().AsSpan()[..self._freeValueCellIndex.Value];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetOrAdd<TKey, TValue>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , Func<TValue> builder
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            self._values[index] = builder();

            return ref self._values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetOrAdd<TKey, TValue, TParam>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , FuncRef<TParam, TValue> builder
            , ref TParam parameter
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            self._values[index] = builder(ref parameter);

            return ref self._values[index];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue RecycleOrAdd<TKey, TValue>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , Func<TValue> builder
            , ActionRef<TValue> recycler
            , PredicateRef<TValue> shouldBeRecycled
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            if (shouldBeRecycled(ref self._values[index]))
                recycler(ref self._values[index]);
            else
                self._values[index] = builder();

            return ref self._values[index];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue RecycleOrAdd<TKey, TValue, TParam>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , FuncRef<TParam, TValue> builder
            , ActionRef<TValue, TParam> recycler
            , PredicateRef<TValue> shouldBeRecycled
            , ref TParam parameter
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            if (shouldBeRecycled(ref self._values[index]))
                recycler(ref self._values[index], ref parameter);
            else
                self._values[index] = builder(ref parameter);

            return ref self._values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetOrAdd<TKey, TValue, TBuilder>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , ref TBuilder builder
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TBuilder : IFunc<TValue>
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            self._values[index] = builder.Invoke();

            return ref self._values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue GetOrAdd<TKey, TValue, TParam, TBuilder>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , ref TBuilder builder
            , ref TParam parameter
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TBuilder : IFuncRef<TParam, TValue>
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            self._values[index] = builder.Invoke(ref parameter);

            return ref self._values[index];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue RecycleOrAdd<TKey, TValue, TBuilder, TRecyler, TShouldBeRecycled>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , ref TBuilder builder
            , ref TRecyler recycler
            , ref TShouldBeRecycled shouldBeRecycled
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TBuilder : IFunc<TValue>
            where TRecyler : IActionRef<TValue>
            where TShouldBeRecycled : IPredicateRef<TValue>
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            if (shouldBeRecycled.Invoke(ref self._values[index]))
                recycler.Invoke(ref self._values[index]);
            else
                self._values[index] = builder.Invoke();

            return ref self._values[index];
        }

        /// <summary>
        /// RecycledOrCreate makes sense to use on maps that are fast cleared and use objects
        /// as value. Once the map is fast cleared, it will try to reuse object values that are
        /// recycled during the fast clearing.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="builder"></param>
        /// <param name="recycler"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="TValueProxy"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue RecycleOrAdd<TKey, TValue, TParam, TBuilder, TRecyler, TShouldBeRecycled>(
              this ref ArrayMapNative<TKey, TValue> self
            , TKey key
            , ref TBuilder builder
            , ref TRecyler recycler
            , ref TShouldBeRecycled shouldBeRecycled
            , ref TParam parameter
        )
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TBuilder : IFuncRef<TParam, TValue>
            where TRecyler : IActionRef<TValue, TParam>
            where TShouldBeRecycled : IPredicateRef<TValue>
        {
            if (self.TryFindIndex(key, out var index))
            {
                self._version.Value++;
                return ref self._values[index];
            }

            self.AddValue(key, out index);

            if (shouldBeRecycled.Invoke(ref self._values[index]))
                recycler.Invoke(ref self._values[index], ref parameter);
            else
                self._values[index] = builder.Invoke(ref parameter);

            return ref self._values[index];
        }
    }

    public static class ArrayMapNativeReadOnlyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TValue> GetValues<TKey, TValue>(this in ArrayMapNative<TKey, TValue>.ReadOnly self)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            => self._values.AsReadOnlySpan()[..self._freeValueCellIndex.Value];
    }
}

#endif
