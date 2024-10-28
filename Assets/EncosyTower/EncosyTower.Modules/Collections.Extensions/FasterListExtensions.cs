using System.Collections.Generic;

namespace EncosyTower.Modules.Collections
{
    public static class FasterListExtensions
    {
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

    }
}
