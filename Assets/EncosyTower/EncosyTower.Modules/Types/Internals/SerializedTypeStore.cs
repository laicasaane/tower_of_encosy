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
using System.Linq;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Logging;
using UnityEngine;

namespace EncosyTower.Modules.Types.Internals
{
    [Serializable]
    internal sealed class SerializedTypeStore : ISerializationCallbackReceiver
    {
        [SerializeField] internal List<string> _assemblyQualifiedNames = new();

        private readonly Dictionary<Type, int> _typeToIndex = new();
        private readonly FasterList<Type> _types = new();
        private bool _initialized;

        public Type this[int index]
        {
            get
            {
                var types = _types.AsSpan();
                return (uint)index < (uint)types.Length ? types[index] : null;
            }
        }

        /// <summary>
        /// Adds a type to the store and returns its index.
        /// </summary>
        /// <param name="type">Type to add</param>
        /// <returns>
        /// Index of the type in the store.
        /// </returns>
        public int Add([NotNull] Type type)
        {
            if (_typeToIndex.TryGetValue(type, out var index) == false)
            {
                index = _types.Count;
                _typeToIndex[type] = index;
                _types.Add(type);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize()
            => Initialize(false);

        public void Initialize(bool forced)
        {
            if (_initialized && forced == false)
            {
                return;
            }

            _initialized = true;

            var assemblyQualifiedNames = _assemblyQualifiedNames;
            var typeToIndex = _typeToIndex;
            var types = _types;

            typeToIndex.Clear();
            types.Clear();
            types.IncreaseCapacityTo(assemblyQualifiedNames.Count);

            for (var i = 0; i < assemblyQualifiedNames.Count; i++)
            {
                var assemblyQualifiedName = assemblyQualifiedNames[i];
                var type = Type.GetType(assemblyQualifiedName, false, false);

                if (type == null)
                {
                    RuntimeLoggerAPI.LogError($"Failed to load type '{assemblyQualifiedName}'.");
                    continue;
                }

                typeToIndex[type] = i;
                types.Add(type);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var assemblyQualifiedNames = _types.Select(static t => t.AssemblyQualifiedName);
            _assemblyQualifiedNames.Clear();
            _assemblyQualifiedNames.AddRange(assemblyQualifiedNames);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Initialize();
        }
    }
}
