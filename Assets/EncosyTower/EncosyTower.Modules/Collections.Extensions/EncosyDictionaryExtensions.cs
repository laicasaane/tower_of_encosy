using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Collections
{
    public static class EncosyDictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict)
            => dict == null || dict.Count < 1;

        public static void AddRange<TKey, TValue>(
              [NotNull] this Dictionary<TKey, TValue> dest
            , IEnumerable<KeyValuePair<TKey, TValue>> collection
        )
        {
            if (collection == null)
            {
                return;
            }

            switch (collection)
            {
                case IReadOnlyCollection<KeyValuePair<TKey, TValue>> roc:
                    dest.EnsureCapacity(dest.Count + roc.Count);
                    break;

                case ICollection<KeyValuePair<TKey, TValue>> c:
                    dest.EnsureCapacity(dest.Count + c.Count);
                    break;
            }

            foreach (var (key, value) in collection)
            {
                dest.TryAdd(key, value);
            }
        }

        public static void AddRangeTo<TKey, TValue>(
              this IEnumerable<KeyValuePair<TKey, TValue>> src
            , ref Dictionary<TKey, TValue> dest
        )
        {
            if (src == null)
            {
                return;
            }

            if (dest == null)
            {
                dest = new(src);
            }
            else
            {
                switch (src)
                {
                    case IReadOnlyCollection<KeyValuePair<TKey, TValue>> roc:
                        dest.EnsureCapacity(dest.Count + roc.Count);
                        break;

                    case ICollection<KeyValuePair<TKey, TValue>> c:
                        dest.EnsureCapacity(dest.Count + c.Count);
                        break;
                }

                foreach (var (key, value) in src)
                {
                    dest.TryAdd(key, value);
                }
            }
        }

        public static void AddRange<TKey, TValue, TKeySrc>(
              [NotNull] this Dictionary<TKey, TValue> dest
            , IEnumerable<KeyValuePair<TKeySrc, TValue>> collection
            , Func<TKeySrc, TKey> convertKey
        )
        {
            if (collection == null || convertKey == null)
            {
                return;
            }

            switch (collection)
            {
                case IReadOnlyCollection<KeyValuePair<TKey, TValue>> roc:
                    dest.EnsureCapacity(dest.Count + roc.Count);
                    break;

                case ICollection<KeyValuePair<TKey, TValue>> c:
                    dest.EnsureCapacity(dest.Count + c.Count);
                    break;
            }

            foreach (var (key, value) in collection)
            {
                dest.TryAdd(convertKey(key), value);
            }
        }

        public static void AddRangeTo<TKey, TValue, TKeySrc>(
              this IEnumerable<KeyValuePair<TKeySrc, TValue>> src
            , ref Dictionary<TKey, TValue> dest
            , Func<TKeySrc, TKey> convertKey
        )
        {
            if (src == null || convertKey == null)
            {
                return;
            }

            dest ??= new();

            switch (src)
            {
                case IReadOnlyCollection<KeyValuePair<TKey, TValue>> roc:
                    dest.EnsureCapacity(dest.Count + roc.Count);
                    break;

                case ICollection<KeyValuePair<TKey, TValue>> c:
                    dest.EnsureCapacity(dest.Count + c.Count);
                    break;
            }

            foreach (var (key, value) in src)
            {
                dest.TryAdd(convertKey(key), value);
            }
        }
    }
}
