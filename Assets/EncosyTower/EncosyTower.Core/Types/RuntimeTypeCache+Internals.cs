#if UNITY_EDITOR && !ENFORCE_ENCOSY_RUNTIME_TYPECACHE
#define __ENCOSY_RUNTIME_TYPECACHE_AUTO__
#endif

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0052 // Remove unread private members

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Ids;
using EncosyTower.Types.Internals;
using UnityEngine;
using UnityEngine.Scripting;

namespace EncosyTower.Types
{
    public static partial class RuntimeTypeCache
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> s_vault = new();

#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
        private static readonly TypeCacheSourceEditor s_source = default;
#else
        private static readonly TypeCacheSourceRuntime s_source;
#endif

        static RuntimeTypeCache()
        {
            InitTypeIdVault();

#if !__ENCOSY_RUNTIME_TYPECACHE_AUTO__
#if UNITY_EDITOR
            s_source = LoadTypeCacheSourceRuntime_EnforcedMode();
#else
            s_source = LoadTypeCacheSourceRuntime();
#endif
#endif
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void InitTypeIdVault()
        {
            TypeIdVault.Init();

            GetInfo<bool>();
            GetInfo<byte>();
            GetInfo<sbyte>();
            GetInfo<char>();
            GetInfo<decimal>();
            GetInfo<double>();
            GetInfo<float>();
            GetInfo<int>();
            GetInfo<uint>();
            GetInfo<long>();
            GetInfo<ulong>();
            GetInfo<short>();
            GetInfo<ushort>();
            GetInfo<string>();
            GetInfo<object>();
            GetInfo<DateTime>();
            GetInfo<DateTimeOffset>();
            GetInfo<TimeSpan>();
            GetInfo<Type>();
            GetInfo<Id>();
            GetInfo<Id2>();
            GetInfo<LongId>();
            GetInfo<TypeId>();
            GetInfo<TypeInfo>();
        }

        [Preserve]
        private static TypeCacheSourceRuntime LoadTypeCacheSourceRuntime()
        {
            var asset = SerializedTypeCacheAsset.GetInstance();
            return new(new(asset._cache));
        }

#if UNITY_EDITOR
#pragma warning disable IDE0051 // Remove unused private members
        private static TypeCacheSourceRuntime LoadTypeCacheSourceRuntime_EnforcedMode()
        {
            var cache = new SerializedTypeCache();
            Editor.SerializedTypeCacheEditor.Regenerate(cache, out _);
            return new(new(cache));
        }
#pragma warning restore IDE0051 // Remove unused private members
#endif

        internal static TypeInfo<T> Register<T>([NotNull] Type type)
        {
            var id = TypeIdVault.Register(type);
            var isUnmanaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;
            var isBlittable = isUnmanaged && type.IsAutoLayout == false && type != typeof(bool);
            var info = new TypeInfo<T>((TypeId<T>)id, type, type.IsValueType, isUnmanaged, isBlittable);
            s_vault.TryAdd(type, (TypeInfo)info);

            return info;
        }
    }
}
