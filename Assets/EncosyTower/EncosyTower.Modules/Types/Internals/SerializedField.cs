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
using System.Reflection;
using UnityEngine;

namespace EncosyTower.Modules.Types.Internals
{
    /// <summary>
    /// Represents in serializable form a reflectable FieldInfo.
    /// </summary>
    [Serializable]
    internal struct SerializedField : ISerializedMember<FieldInfo>
    {
        [SerializeField] private string _fieldName;
        [SerializeField] private SerializedType _declaringType;
        [SerializeField] private BindingFlags _bindingFlags;

        public SerializedField(FieldInfo memberInfo, SerializedTypeStore typeStore)
        {
            _declaringType = new SerializedType(memberInfo.DeclaringType, typeStore);
            _fieldName = memberInfo.Name;

            BindingFlags bindingFlags = default;

            if (memberInfo.IsPublic)
            {
                bindingFlags |= BindingFlags.Public;
            }
            else
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            if (memberInfo.IsStatic)
            {
                bindingFlags |= BindingFlags.Static;
            }
            else
            {
                bindingFlags |= BindingFlags.Instance;
            }

            _bindingFlags = bindingFlags;
        }

        public readonly FieldInfo Deserialize(SerializedTypeStore typeStore)
        {
            var declaringType = _declaringType.Deserialize(typeStore);
            Checks.IsTrue(declaringType != null, "declaringType is null");
            return declaringType.GetField(_fieldName, _bindingFlags);
        }
    }
}
