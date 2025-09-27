using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace EncosyTower.Types
{
    /// <summary>
    /// Provides methods for fast type extraction from assemblies loaded into the Unity Domain.
    /// </summary>
    [Preserve]
    public static partial class RuntimeTypeCache
    {
        /// <summary>
        /// Retrieves information about the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// Returns a <see cref="TypeInfo"/> which contains information about <typeparamref name="T"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly TypeInfo<T> GetInfo<T>()
            => ref Type<T>.Info;

        /// <summary>
        /// Retrieves information about the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// Returns a <see cref="TypeInfo"/> which contains information about <paramref name="type"/>.
        /// </returns>
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

        /// <summary>
        /// Retrieves an unordered collection of types derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of a class or interface.</typeparam>
        /// <returns>
        /// Returns an unordered collection of derived types.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom<T>()
            => s_source.GetTypesDerivedFrom<T>();

        /// <summary>
        /// Retrieves an unordered collection of types derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of a class or interface.</typeparam>
        /// <param name="assemblyName">Optional assembly name.</param>
        /// <returns>
        /// Returns an unordered collection of derived types defined in this <paramref name="assemblyName"/>.
        /// </returns>
        /// <remarks>
        /// Literals should be passed into the method.
        /// Source generator accompanied RuntimeTypeCache can't resolve non-literal variables.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom<T>([NotNull] string assemblyName)
            => s_source.GetTypesDerivedFrom<T>(assemblyName);

        /// <summary>
        /// Retrieves an unordered collection of types derived from <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of a class or interface.</param>
        /// <returns>
        /// Returns an unordered collection of derived types.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom([NotNull] Type type)
            => s_source.GetTypesDerivedFrom(type);

        /// <summary>
        /// Retrieves an unordered collection of types derived from <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of a class or interface.</param>
        /// <param name="assemblyName">Optional assembly name.</param>
        /// <returns>
        /// Returns an unordered collection of derived types defined in this <paramref name="assemblyName"/>.
        /// </returns>
        /// <remarks>
        /// Literals should be passed into the method.
        /// Source generator accompanied RuntimeTypeCache can't resolve non-literal variables.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesDerivedFrom([NotNull] Type type, [NotNull] string assemblyName)
            => s_source.GetTypesDerivedFrom(type, assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute<T>() where T : Attribute
            => s_source.GetTypesWithAttribute<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute<T>([NotNull] string assemblyName) where T : Attribute
            => s_source.GetTypesWithAttribute<T>(assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute([NotNull] Type attrType)
            => s_source.GetTypesWithAttribute(attrType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Type> GetTypesWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
            => s_source.GetTypesWithAttribute(attrType, assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>() where T : Attribute
            => s_source.GetFieldsWithAttribute<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>([NotNull] string assemblyName) where T : Attribute
            => s_source.GetFieldsWithAttribute<T>(assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute([NotNull] Type attrType)
            => s_source.GetFieldsWithAttribute(attrType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
            => s_source.GetFieldsWithAttribute(attrType, assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
            => s_source.GetMethodsWithAttribute<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>([NotNull] string assemblyName) where T : Attribute
            => s_source.GetMethodsWithAttribute<T>(assemblyName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute([NotNull] Type attrType)
            => s_source.GetMethodsWithAttribute(attrType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute([NotNull] Type attrType, [NotNull] string assemblyName)
            => s_source.GetMethodsWithAttribute(attrType, assemblyName);
    }
}
