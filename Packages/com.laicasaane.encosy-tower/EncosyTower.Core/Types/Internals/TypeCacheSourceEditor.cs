#if UNITY_EDITOR && !ENFORCE_ENCOSY_RUNTIME_TYPECACHE
#define __RUNTIME_TYPECACHE_AUTO__
#endif

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

#if __RUNTIME_TYPECACHE_AUTO__

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EncosyTower.Types.Internals
{
    internal readonly struct TypeCacheSourceEditor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom<T>()
        {
            var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom<T>(string assemblyName)
        {
            var items = UnityEditor.TypeCache.GetTypesDerivedFrom<T>(assemblyName);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type)
        {
            var items = UnityEditor.TypeCache.GetTypesDerivedFrom(type);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type, string assemblyName)
        {
            var items = UnityEditor.TypeCache.GetTypesDerivedFrom(type, assemblyName);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>();
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute<T>(string assemblyName) where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetTypesWithAttribute<T>(assemblyName);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType)
        {
            var items = UnityEditor.TypeCache.GetTypesWithAttribute(attrType);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType, string assemblyName)
        {
            var items = UnityEditor.TypeCache.GetTypesWithAttribute(attrType, assemblyName);
            return TypeCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>() where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>();
            return FieldCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute<T>(string assemblyName) where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetFieldsWithAttribute<T>(assemblyName);
            return FieldCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType)
        {
            var items = UnityEditor.TypeCache.GetFieldsWithAttribute(attrType);
            return FieldCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType, string assemblyName)
        {
            var items = UnityEditor.TypeCache.GetFieldsWithAttribute(attrType, assemblyName);
            return FieldCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>() where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>();
            return MethodCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute<T>(string assemblyName) where T : Attribute
        {
            var items = UnityEditor.TypeCache.GetMethodsWithAttribute<T>(assemblyName);
            return MethodCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType)
        {
            var items = UnityEditor.TypeCache.GetMethodsWithAttribute(attrType);
            return MethodCollection.AsMemory(items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType, string assemblyName)
        {
            var items = UnityEditor.TypeCache.GetMethodsWithAttribute(attrType, assemblyName);
            return MethodCollection.AsMemory(items);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct TypeCollection
        {
            [FieldOffset(0)] public UnityEditor.TypeCache.TypeCollection collection;
            [FieldOffset(0)] public Type[] items;

            public static ReadOnlyMemory<Type> AsMemory(UnityEditor.TypeCache.TypeCollection value)
            {
                var result = new TypeCollection { collection = value };
                return result.items.AsMemory();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FieldCollection
        {
            [FieldOffset(0)] public UnityEditor.TypeCache.FieldInfoCollection collection;
            [FieldOffset(0)] public FieldInfo[] items;

            public static ReadOnlyMemory<FieldInfo> AsMemory(UnityEditor.TypeCache.FieldInfoCollection value)
            {
                var result = new FieldCollection { collection = value };
                return result.items.AsMemory();
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MethodCollection
        {
            [FieldOffset(0)] public UnityEditor.TypeCache.MethodCollection collection;
            [FieldOffset(0)] public MethodInfo[] items;

            public static ReadOnlyMemory<MethodInfo> AsMemory(UnityEditor.TypeCache.MethodCollection value)
            {
                var result = new MethodCollection { collection = value };
                return result.items.AsMemory();
            }
        }
    }
}

#endif
