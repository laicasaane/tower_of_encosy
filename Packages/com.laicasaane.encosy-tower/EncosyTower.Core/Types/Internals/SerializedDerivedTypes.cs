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
using UnityEngine;

namespace EncosyTower.Types.Internals
{
    /// <summary>
    /// Represents in serializable form all types that derive from a specific parent type.
    /// </summary>
    [Serializable]
    internal class SerializedDerivedTypes
    {
        [SerializeField] internal string _assemblyName = string.Empty;
        [SerializeField] internal SerializedType _baseType;
        [SerializeField] internal List<SerializedType> _derivedTypes = new();
    }
}
