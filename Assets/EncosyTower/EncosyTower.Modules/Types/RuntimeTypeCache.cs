namespace EncosyTower.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine.Scripting;

    /// <summary>
    /// Provides information about a type.
    /// </summary>
    public static partial class RuntimeTypeCache
    {
        /// <summary>
        /// Gets the <see cref="Type"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type Get<T>()
            => Type<T>.Value;

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is a value type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns>
        /// <see langword="true"/> if <typeparamref name="T"/> is a value type; otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Is<T>(this Type type)
            => type == Type<T>.Value;

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is a value type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// <see langword="true"/> if <typeparamref name="T"/> is a value type; otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType<T>()
            => Type<T>.IsValueType;

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is unmanaged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// <see langword="true"/> if <typeparamref name="T"/> is unmanaged; otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnmanaged<T>()
            => Type<T>.IsUnmanaged;

        /// <summary>
        /// Determines whether <typeparamref name="T"/> is both unmanaged and blittable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// <see langword="true"/> if <typeparamref name="T"/> is both unmanaged and blittable; otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlittable<T>()
            => Type<T>.IsBlittable;

        /// <summary>
        /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static TypeId<T> GetId<T>()
        {
            var id = new TypeId<T>(TypeIdVault.Cache<T>.Id);
            TypeIdVault.Register(id._value, Type<T>.Value);
            return id;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeInfo<T> GetInfo<T>()
            => Type<T>.Info;

        /// <summary>
        /// Gets the hash code of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// The hash code of <typeparamref name="T"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeHash GetHash<T>()
            => Type<T>.Hash;

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
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom(typeof(T));
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
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>(assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesDerivedFrom(typeof(T), assemblyName);
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
#if UNITY_EDITOR
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
#if UNITY_EDITOR
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
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>();
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute(typeof(T));
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>(assemblyName);
                return TypeCollection.AsMemory(items);
            }
#else
            {
                return GetTypesWithAttribute(typeof(T), assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute([NotNull] Type attrType)
        {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
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
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>();
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute(typeof(T));
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>(assemblyName);
                return FieldCollection.AsMemory(items);
            }
#else
            {
                return GetFieldsWithAttribute(typeof(T), assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute([NotNull] Type attrType)
        {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
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
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>();
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute(typeof(T));
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>([NotNull] string assemblyName)
            where T : Attribute
        {
#if UNITY_EDITOR
            {
                var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>(assemblyName);
                return MethodCollection.AsMemory(items);
            }
#else
            {
                return GetMethodsWithAttribute(typeof(T), assemblyName);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute([NotNull] Type attrType)
        {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
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

#if UNITY_EDITOR

namespace EncosyTower.Modules
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
