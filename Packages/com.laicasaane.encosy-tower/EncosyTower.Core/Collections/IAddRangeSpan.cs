using System;

namespace EncosyTower.Collections
{
    public interface IAddRangeSpan<T>
    {
        void AddRange(ReadOnlySpan<T> items);

        void AddRange(ReadOnlySpan<T> items, int count);
    }
}
