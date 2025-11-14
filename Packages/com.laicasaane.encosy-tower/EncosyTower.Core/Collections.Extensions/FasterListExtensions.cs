using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Collections.Extensions
{
    public static class FasterListExtensions
    {
        /// <remarks>
        /// This method allocates an internal <see cref="FasterListProxy{T}"/>
        /// which implements <see cref="IListProxy{T}"/> for <paramref name="self"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProxiedList<T> ToProxiedList<T>([NotNull] this FasterList<T> self)
            => new(new FasterListProxy<T>(self));

        public static void AddRangeTo<T>(this IEnumerable<T> src, ref FasterList<T> list)
        {
            if (src == null)
            {
                return;
            }

            list ??= new();
            list._version++;

            if (src is ICollection<T> c)
            {
                var count = c.Count;

                if (count == 0)
                {
                    return;
                }

                if (list._count + count > list._buffer.Length)
                {
                    list.AllocateMore(list._count + count);
                }

                c.CopyTo(list._buffer, list._count);
                list._count += count;
            }
            else
            {
                foreach (var item in src)
                {
                    list.Add(item);
                }
            }
        }

        internal sealed class FasterListProxy<T> : IListProxy<T>
        {
            internal readonly FasterList<T> _list;

            public FasterListProxy(FasterList<T> list)
            {
                _list = list;
            }

            public T[] Items
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _list._buffer;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _list._buffer = value;
            }

            public ref int Size
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _list._count;
            }

            public ref int Version
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _list._version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Resize(int newCapacity, bool _)
                => Resize(newCapacity);

            public void Resize(int newCapacity)
            {
                var newList = new T[newCapacity];
                if (_list._count > 0) Array.Copy(_list._buffer, newList, _list._count);
                _list._buffer = newList;
            }
        }
    }
}
