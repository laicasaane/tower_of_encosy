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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;

namespace EncosyTower.Types.Internals
{
    internal sealed class DeserializedTypeCache
    {
        internal readonly AssemblyNameToMemberMapMap<Type> _typesDerivedFromTypeMap;
        internal readonly AssemblyNameToMemberMapMap<Type> _typesWithAttributeMap;
        internal readonly AssemblyNameToMemberMapMap<FieldInfo> _fieldsWithAttributeMap;
        internal readonly AssemblyNameToMemberMapMap<MethodInfo> _methodsWithAttributeMap;

        public DeserializedTypeCache([NotNull] SerializedTypeCache cache)
        {
            _typesDerivedFromTypeMap = MapFromSerialized(cache, cache._typesDerivedFromTypeList);
            _typesWithAttributeMap = MapFromSerialized<SerializedType, Type>(cache, cache._typesWithAttributeList);
            _fieldsWithAttributeMap = MapFromSerialized<SerializedField, FieldInfo>(cache, cache._fieldsWithAttributeList);
            _methodsWithAttributeMap = MapFromSerialized<SerializedMethod, MethodInfo>(cache, cache._methodsWithAttributeList);
        }

        private static AssemblyNameToMemberMapMap<Type> MapFromSerialized(
              SerializedTypeCache cache
            , List<SerializedDerivedTypes> serializedItems
        )
        {
            var map = new AssemblyNameToMemberMapMap<Type>();
            var items = serializedItems.AsSpan();
            var itemsLength = items.Length;
            var typeStore = cache._typeStore;

            for (var i = 0; i < itemsLength; i++)
            {
                var item = items[i];
                var baseType = item._baseType.Deserialize(typeStore);

                if (baseType is null)
                {
                    continue;
                }

                var assemblyName = string.IsNullOrWhiteSpace(item._assemblyName)
                    ? string.Empty
                    : item._assemblyName;

                if (map.TryGetValue(assemblyName, out var memberMap) == false)
                {
                    memberMap = new TypeToMemberMap<Type>();
                    map[assemblyName] = memberMap;
                }

                if (memberMap.TryGetValue(baseType, out var types) == false)
                {
                    types = new FasterList<Type>();
                    memberMap[baseType] = types;
                }

                Deserialize(typeStore, types, item._derivedTypes.AsSpan());
            }

            return map;
        }

        private static AssemblyNameToMemberMapMap<T2> MapFromSerialized<T1, T2>(
              SerializedTypeCache cache
            , List<SerializedAnnotatedMembers<T1>> serializedItems
        )
            where T1 : struct, ISerializedMember<T2>
            where T2 : MemberInfo
        {
            var map = new AssemblyNameToMemberMapMap<T2>();
            var items = serializedItems.AsSpan();
            var itemsLength = items.Length;
            var typeStore = cache._typeStore;

            for (var i = 0; i < itemsLength; i++)
            {
                var item = items[i];
                var attributeType = item._attributeType.Deserialize(typeStore);

                if (attributeType is null)
                {
                    continue;
                }

                var assemblyName = string.IsNullOrWhiteSpace(item._assemblyName)
                    ? string.Empty
                    : item._assemblyName;

                if (map.TryGetValue(assemblyName, out var memberMap) == false)
                {
                    memberMap = new TypeToMemberMap<T2>();
                    map[assemblyName] = memberMap;
                }

                if (memberMap.TryGetValue(attributeType, out var members) == false)
                {
                    members = new FasterList<T2>();
                    memberMap[attributeType] = members;
                }

                Deserialize(typeStore, members, item._matches.AsSpan());
            }

            return map;
        }

        private static void Deserialize<T1, T2>(
              SerializedTypeStore typeStore
            , FasterList<T2> output
            , Span<T1> items
        )
            where T1 : struct, ISerializedMember<T2>
            where T2 : MemberInfo
        {
            var itemsLength = items.Length;

            output.IncreaseCapacityTo(output.Count + itemsLength);

            for (var i = 0; i < itemsLength; i++)
            {
                var item = items[i];
                var result = item.Deserialize(typeStore);

                if (result is not null)
                {
                    output.Add(result);
                }
            }
        }

        internal sealed class TypeToMemberMap<T> : Dictionary<Type, FasterList<T>>
            where T : MemberInfo
        {
        }

        // TODO: Assembly Name should be replaced by an integer hash so this map can be performant.
        // However to facilitate such thing, we have to wait for the new CoreCLR and higher .NET version
        // to incorporate interceptors into source-generated code.
        // https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#interceptors
        //
        // The idea is we can generate a method that passes a stable hash for the assembly name
        // into this map. The code for stable hash is already inside the csproj for EncosyTower.SourceGen.
        //
        // Example:
        // For this call:
        //     void DoSomething()
        //     {
        //         RuntimeTypeCache.GetTypesDerivedFromType<SomeType>("Some.Assembly.Name");
        //     }
        //
        // We can then generate a intercept method:
        //     [InterceptsLocation(<file-name.cs>, <line>, <column>)]
        //     public static void DoSomething_RuntimeTypeCache_GetTypesDerivedFromType_SomeType_Some_Assembly_Name()
        //     {
        //         RuntimeTypeCache.GetTypesDerivedFromType<SomeType>(1234567890);
        //     }
        internal sealed class AssemblyNameToMemberMapMap<T> : Dictionary<string, TypeToMemberMap<T>>
            where T : MemberInfo
        {
        }
    }
}
