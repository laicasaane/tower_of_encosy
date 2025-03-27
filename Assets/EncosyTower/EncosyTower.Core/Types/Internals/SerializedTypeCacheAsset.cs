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

using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Types.Internals
{
    internal class SerializedTypeCacheAsset : ScriptableObject
    {
        private static SerializedTypeCacheAsset s_instance;

        [SerializeField] internal SerializedTypeCache _cache = new();

        public static SerializedTypeCacheAsset GetInstance()
        {
            if (s_instance.IsInvalid())
            {
                try
                {
                    s_instance = Resources.Load<SerializedTypeCacheAsset>(nameof(SerializedTypeCacheAsset));
                }
                catch
                {
                    throw GetMissingReferenceException();
                }
                finally
                {
                    if (s_instance.IsInvalid())
                    {
                        throw GetMissingReferenceException();
                    }
                }
            }

            return s_instance;

            static MissingReferenceException GetMissingReferenceException()
            {
                return new MissingReferenceException(
                    $"Cannot find asset of type {nameof(SerializedTypeCacheAsset)}."
                );
            }
        }

#if UNITY_EDITOR
        internal static void InitWhenDomainReloadDisabled()
        {
            s_instance = null;
        }
#endif
    }
}
