using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Vaults
{
#if UNITY_EDITOR
    internal static partial class GlobalMapEditor
    {
        private readonly static Dictionary<TypeId, IDictionary> s_maps = new();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            var maps = s_maps;

            foreach (var (_, map) in maps)
            {
                map?.Clear();
            }
        }

        public static void Register<TScope>(IDictionary map)
        {
            s_maps.TryAdd((TypeId)TypeId<TScope>.Value, map);
        }
    }
#endif

    public static partial class GlobalMap<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        private static readonly Dictionary<TKey, TValue> s_map = new(4);

#if UNITY_EDITOR
        static GlobalMap()
        {
            GlobalMapEditor.Register<Scope>(s_map);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove(TKey key)
            => s_map.Remove(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(TKey key, TValue value)
            => s_map[key] = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd(TKey key, TValue value)
            => s_map.TryAdd(key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(TKey key, out TValue value)
            => s_map.TryGetValue(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(TKey key)
            => s_map.ContainsKey(key);

        private readonly struct Scope { }
    }
}
