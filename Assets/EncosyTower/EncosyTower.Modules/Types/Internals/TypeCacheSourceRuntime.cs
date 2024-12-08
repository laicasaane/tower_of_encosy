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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules.Types.Internals
{
    internal readonly struct TypeCacheSourceRuntime
    {
        private readonly DeserializedTypeCache _cache;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeCacheSourceRuntime([NotNull] DeserializedTypeCache cache)
        {
            _cache = cache;
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

        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type)
        {
            return Array.Empty<Type>();
        }

        public ReadOnlyMemory<Type> GetTypesDerivedFrom(Type type, string assemblyName)
        {
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

        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType)
        {
            return Array.Empty<Type>();
        }

        public ReadOnlyMemory<Type> GetTypesWithAttribute(Type attrType, string assemblyName)
        {
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

        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType)
        {
            return Array.Empty<FieldInfo>();
        }

        public ReadOnlyMemory<FieldInfo> GetFieldsWithAttribute(Type attrType, string assemblyName)
        {
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

        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType)
        {
            return Array.Empty<MethodInfo>();
        }

        public ReadOnlyMemory<MethodInfo> GetMethodsWithAttribute(Type attrType, string assemblyName)
        {
            return Array.Empty<MethodInfo>();
        }
    }
}
