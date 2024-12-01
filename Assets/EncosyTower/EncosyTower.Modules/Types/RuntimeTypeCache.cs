#if UNITY_EDITOR && !ENFORCE_ENCOSY_RUNTIME_TYPECACHE
#define __ENCOSY_RUNTIME_TYPECACHE_AUTO__
#endif

namespace EncosyTower.Modules.Types
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides methods for fast type extraction from assemblies loaded into the Unity Domain.
    /// </summary>
    public static partial class RuntimeTypeCache
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> s_vault = new();

        static RuntimeTypeCache()
        {
            Init();
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void Init()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TypeInfo<T> GetInfo<T>()
            => ref Type<T>.Info;

        public static TypeInfo GetInfo([NotNull] Type type)
        {
            if (s_vault.TryGetValue(type, out var info))
            {
                return info;
            }

            var id = TypeIdVault.Register(type);
            var isUnmanaged = type.IsReferenceOrContainsReferences() == false;
            var isBlittable = isUnmanaged && type.IsAutoLayout == false && type != typeof(bool);
            info = new TypeInfo(id, type, type.IsValueType, isUnmanaged, isBlittable);
            s_vault.TryAdd(type, info);
            return info;
        }

        internal static TypeInfo<T> Register<T>([NotNull] Type type)
        {
            var id = TypeIdVault.Register(type);
            var isUnmanaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;
            var isBlittable = isUnmanaged && type.IsAutoLayout == false && type != typeof(bool);
            var info = new TypeInfo<T>((TypeId<T>)id, type, type.IsValueType, isUnmanaged, isBlittable);
            s_vault.TryAdd(type, (TypeInfo)info);

            return info;
        }

        /// <summary>
        /// Retrieves an unordered collection of types derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of a class or interface.</typeparam>
        /// <returns>
        /// Returns an unordered collection of derived types.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom<T>()
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom(Type<T>.Value);
            }
#endif
        }

        /// <summary>
        /// Retrieves an unordered collection of types derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of a class or interface.</typeparam>
        /// <param name="assemblyName">	Optional assembly name.</param>
        /// <returns>
        /// Returns an unordered collection of derived types defined in this <paramref name="assemblyName"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom<T>([NotNull] string assemblyName)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>(assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom(Type<T>.Value, assemblyName);
            }
#endif
        }

        /// <summary>
        /// Retrieves an unordered collection of types derived from <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of a class or interface.</param>
        /// <returns>
        /// Returns an unordered collection of derived types.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom([NotNull] Type type)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom(type);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom_Internal(type);
            }
#endif
        }

        /// <summary>
        /// Retrieves an unordered collection of types derived from <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of a class or interface.</param>
        /// <param name="assemblyName">	Optional assembly name.</param>
        /// <returns>
        /// Returns an unordered collection of derived types defined in this <paramref name="assemblyName"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom([NotNull] Type type, [NotNull] string assemblyName)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom(type, assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom_Internal(type, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute<T>()
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>();
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute(Type<T>.Value);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>(assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute(Type<T>.Value, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute([NotNull] Type attrType)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute(attrType);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute_Internal(attrType);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute(attrType, assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute_Internal(attrType, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>()
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>();
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute(Type<T>.Value);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>(assemblyName);
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute(Type<T>.Value, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute([NotNull] Type attrType)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute(attrType);
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute_Internal(attrType);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute(attrType, assemblyName);
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute_Internal(attrType, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>()
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>();
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute(Type<T>.Value);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>(assemblyName);
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute(Type<T>.Value, assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute([NotNull] Type attrType)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute(attrType);
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute_Internal(attrType);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
        {
#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute(attrType, assemblyName);
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute_Internal(attrType, assemblyName);
            }
#endif
        }

        [Preserve]
        private static ReadOnlyMemory<Type> GetTypesDerivedFrom_Internal(Type type)
        {
            return Array.Empty<Type>();
        }

        [Preserve]
        private static ReadOnlyMemory<Type> GetTypesDerivedFrom_Internal(Type type, string assemblyName)
        {
            return Array.Empty<Type>();
        }

        [Preserve]
        private static ReadOnlyMemory<Type> GetTypesWithAttribute_Internal(Type attrType)
        {
            return Array.Empty<Type>();
        }

        [Preserve]
        private static ReadOnlyMemory<Type> GetTypesWithAttribute_Internal(Type attrType, string assemblyName)
        {
            return Array.Empty<Type>();
        }

        [Preserve]
        private static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute_Internal(Type attrType)
        {
            return Array.Empty<FieldInfo>();
        }

        [Preserve]
        private static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute_Internal(Type attrType, string assemblyName)
        {
            return Array.Empty<FieldInfo>();
        }

        [Preserve]
        private static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute_Internal(Type attrType)
        {
            return Array.Empty<MethodInfo>();
        }

        [Preserve]
        private static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute_Internal(Type attrType, string assemblyName)
        {
            return Array.Empty<MethodInfo>();
        }
    }
}

#if __ENCOSY_RUNTIME_TYPECACHE_AUTO__

namespace EncosyTower.Modules.Types
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEditor;

    static partial class RuntimeTypeCache
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct TypeCollection
        {
            [FieldOffset(0)] public TypeCache.TypeCollection collection;
            [FieldOffset(0)] public Type[] items;

            public static ReadOnlyMemory<Type> AsMemory(TypeCache.TypeCollection value)
            {
                var result = new TypeCollection { collection = value };
                return result.items.AsMemory();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FieldCollection
        {
            [FieldOffset(0)] public TypeCache.FieldInfoCollection collection;
            [FieldOffset(0)] public FieldInfo[] items;

            public static ReadOnlyMemory<FieldInfo> AsMemory(TypeCache.FieldInfoCollection value)
            {
                var result = new FieldCollection { collection = value };
                return result.items.AsMemory();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MethodCollection
        {
            [FieldOffset(0)] public TypeCache.MethodCollection collection;
            [FieldOffset(0)] public MethodInfo[] items;

            public static ReadOnlyMemory<MethodInfo> AsMemory(TypeCache.MethodCollection value)
            {
                var result = new MethodCollection { collection = value };
                return result.items.AsMemory();
            }
        }
    }
}

#endif
