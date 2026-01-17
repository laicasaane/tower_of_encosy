// MIT License
//
// Copyright (c) 2024 Mika Notarnicola
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// https://github.com/thebeardphantom/Runtime-TypeCache

#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Collections.Extensions.Unsafe;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Types.Internals
{
    internal readonly struct TypeCacheSourceRuntime : IIsCreated
    {
        private readonly DeserializedTypeCache _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeCacheSourceRuntime([NotNull] DeserializedTypeCache cache)
        {
            _cache = cache;
        }

        public bool IsCreated
        {
            get => _cache != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom<T>()
        {
            return GetTypesDerivedFrom(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom<T>(string assemblyName)
        {
            return GetTypesDerivedFrom(typeof(T), assemblyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type)
        {
            return GetTypesDerivedFrom(type, string.Empty);
        }

        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type, string assemblyName)
        {
            ThrowIfNull(_cache);

            EnsureValidAssemblyName(ref assemblyName);

            var assemblyToMemberMap = _cache._typesDerivedFromTypeMap;

            if (assemblyToMemberMap.TryGetValue(assemblyName, out var memberMap)
                && memberMap.TryGetValue(type, out var members)
            )
            {
                members.GetBufferUnsafe(out var buffer, out var count);
                return buffer.AsMemory(0, count);
            }

            return Array.Empty<Type>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            return GetTypesWithAttribute(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute<T>(string assemblyName) where T : Attribute
        {
            return GetTypesWithAttribute(typeof(T), assemblyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType)
        {
            return GetTypesWithAttribute(attrType, string.Empty);
        }

        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType, string assemblyName)
        {
            ThrowIfNull(_cache);

            EnsureValidAssemblyName(ref assemblyName);

            var assemblyToMemberMap = _cache._typesWithAttributeMap;

            if (assemblyToMemberMap.TryGetValue(assemblyName, out var memberMap)
                && memberMap.TryGetValue(attrType, out var members)
            )
            {
                members.GetBufferUnsafe(out var buffer, out var count);
                return buffer.AsMemory(0, count);
            }

            return Array.Empty<Type>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>() where T : Attribute
        {
            return GetFieldsWithAttribute(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>(string assemblyName) where T : Attribute
        {
            return GetFieldsWithAttribute(typeof(T), assemblyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType)
        {
            return GetFieldsWithAttribute(attrType, string.Empty);
        }

        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType, string assemblyName)
        {
            ThrowIfNull(_cache);

            EnsureValidAssemblyName(ref assemblyName);

            var assemblyToMemberMap = _cache._fieldsWithAttributeMap;

            if (assemblyToMemberMap.TryGetValue(assemblyName, out var memberMap)
                && memberMap.TryGetValue(attrType, out var members)
            )
            {
                members.GetBufferUnsafe(out var buffer, out var count);
                return buffer.AsMemory(0, count);
            }

            return Array.Empty<FieldInfo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
        {
            return GetMethodsWithAttribute(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>(string assemblyName) where T : Attribute
        {
            return GetMethodsWithAttribute(typeof(T), assemblyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType)
        {
            return GetMethodsWithAttribute(attrType, string.Empty);
        }

        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType, string assemblyName)
        {
            ThrowIfNull(_cache);

            EnsureValidAssemblyName(ref assemblyName);

            var assemblyToMemberMap = _cache._methodsWithAttributeMap;

            if (assemblyToMemberMap.TryGetValue(assemblyName, out var memberMap)
                && memberMap.TryGetValue(attrType, out var members)
            )
            {
                members.GetBufferUnsafe(out var buffer, out var count);
                return buffer.AsMemory(0, count);
            }

            return Array.Empty<MethodInfo>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureValidAssemblyName(ref string assemblyName)
        {
            assemblyName = string.IsNullOrWhiteSpace(assemblyName) ? string.Empty : assemblyName;
        }

        [HideInCallstack, StackTraceHidden, Conditional("__ENCOSY_VALIDATION__")]
        private static void ThrowIfNull(DeserializedTypeCache cache)
        {
            if (cache == null)
            {
                throw new InvalidOperationException("RuntimeTypeCache is not initialized correctly.");
            }
        }
    }
}
