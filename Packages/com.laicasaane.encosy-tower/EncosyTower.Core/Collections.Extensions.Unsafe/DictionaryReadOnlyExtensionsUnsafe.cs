using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections.Extensions.Unsafe
{
    public static class DictionaryReadOnlyExtensionsUnsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Dictionary<TKey, TValue> GetDictionaryUnsafe<TKey, TValue>(this DictionaryReadOnly<TKey, TValue> dict)
        {
            ThrowHelper.ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(dict.IsCreated);
            return dict._dictionary.Dictionary;
        }
    }
}
