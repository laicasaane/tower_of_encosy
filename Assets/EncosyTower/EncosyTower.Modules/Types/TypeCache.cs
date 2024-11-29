using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace EncosyTower.Modules
{
    public static class TypeCache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type Get<T>()
            => TypeCache<T>.Type;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Is<T>(this Type type)
            => type == TypeCache<T>.Type;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeHash GetHash<T>()
            => TypeCache<T>.Hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName<T>()
            => TypeCache<T>.Type.Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnmanaged<T>()
            => RuntimeHelpers.IsReferenceOrContainsReferences<T>() == false;

        public static TypeId GetId<T>()
        {
            var id = new TypeId(TypeIdVault.Cache<T>.Id);
            TypeIdVault.Register(id._value, TypeCache<T>.Type);
            return id;
        }

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

#if UNITY_EDITOR
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct TypeCollection
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public UnityEditor.TypeCache.TypeCollection collection;
            [System.Runtime.InteropServices.FieldOffset(0)] public Type[] types;

            public static ReadOnlyMemory<Type> AsMemory(UnityEditor.TypeCache.TypeCollection collection)
            {
                var result = new TypeCollection { collection = collection };
                return result.types.AsMemory();
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct FieldCollection
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public UnityEditor.TypeCache.FieldInfoCollection collection;
            [System.Runtime.InteropServices.FieldOffset(0)] public FieldInfo[] fields;

            public static ReadOnlyMemory<FieldInfo> AsMemory(UnityEditor.TypeCache.FieldInfoCollection collection)
            {
                var result = new FieldCollection { collection = collection };
                return result.fields.AsMemory();
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct MethodCollection
        {
            [System.Runtime.InteropServices.FieldOffset(0)] public UnityEditor.TypeCache.MethodCollection collection;
            [System.Runtime.InteropServices.FieldOffset(0)] public MethodInfo[] methods;

            public static ReadOnlyMemory<MethodInfo> AsMemory(UnityEditor.TypeCache.MethodCollection collection)
            {
                var result = new MethodCollection { collection = collection };
                return result.methods.AsMemory();
            }
        }
#endif
    }
}
