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
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Collections.Unsafe;

namespace EncosyTower.Modules.Types.Internals
{
    internal sealed class DeserializedTypeCache
    {
        internal readonly Dictionary<Type, FasterList<Type>> _typesDerivedFromTypeMap;
        internal readonly Dictionary<Type, FasterList<Type>> _typesWithAttributeMap;
        internal readonly Dictionary<Type, FasterList<FieldInfo>> _fieldsWithAttributeMap;
        internal readonly Dictionary<Type, FasterList<MethodInfo>> _methodsWithAttributeMap;

        public DeserializedTypeCache([NotNull] SerializedTypeCache cache)
        {
            _typesDerivedFromTypeMap = MapFromSerialized(cache, cache._typesDerivedFromTypeList);
            _typesWithAttributeMap = MapFromSerialized<SerializedType, Type>(cache, cache._typesWithAttributeList);
            _fieldsWithAttributeMap = MapFromSerialized<SerializedField, FieldInfo>(cache, cache._fieldsWithAttributeList);
            _methodsWithAttributeMap = MapFromSerialized<SerializedMethod, MethodInfo>(cache, cache._methodsWithAttributeList);
        }

        private static Dictionary<Type, FasterList<Type>> MapFromSerialized(
              SerializedTypeCache cache
            , List<SerializedDerivedTypes> serializedItems
        )
        {
            var lookup = new Dictionary<Type, FasterList<Type>>();
            var items = serializedItems.AsSpanUnsafe();
            var itemsLength = items.Length;
            var typeStore = cache._typeStore;

            for (var i = 0; i < itemsLength; i++)
            {
                var item = items[i];
                var parentType = item._parentType.Deserialize(typeStore);

                if (parentType is null)
                {
                    continue;
                }

                if (lookup.TryGetValue(parentType, out var types) == false)
                {
                    types = new FasterList<Type>();
                    lookup[parentType] = types;
                }

                Deserialize(typeStore, types, item._derivedTypes.AsSpanUnsafe());
            }

            return lookup;
        }

        private static Dictionary<Type, FasterList<T2>> MapFromSerialized<T1, T2>(
              SerializedTypeCache cache
            , List<SerializedDecoratedMembers<T1>> serializedItems
        )
            where T1 : struct, ISerializedMember<T2>
            where T2 : MemberInfo
        {
            var lookup = new Dictionary<Type, FasterList<T2>>();
            var items = serializedItems.AsSpanUnsafe();
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

                if (lookup.TryGetValue(attributeType, out var members) == false)
                {
                    members = new FasterList<T2>();
                    lookup[attributeType] = members;
                }

                Deserialize(typeStore, members, item._matches.AsSpanUnsafe());
            }

            return lookup;
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
    }
}
