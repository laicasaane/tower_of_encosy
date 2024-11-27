// https://github.com/hadashiA/UniTaskPubSub/blob/master/Assets/UniTaskPubSub/Runtime/Internal/CappedArrayPool.cs

// Copyright(c) 2020 hadashiA
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

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules.Collections
{
    public sealed class CappedArrayPool<T>
    {
        internal const int INITIAL_BUCKET_SIZE = 4;

        public static CappedArrayPool<T> Shared8Limit => s_shared8Limit;

        private readonly static bool s_isTManaged = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        private static CappedArrayPool<T> s_shared8Limit;

        private readonly T[][][] _buckets;
        private readonly object _syncRoot = new();
        private readonly int[] _tails;

        static CappedArrayPool()
        {
            Init();
        }

        /// <seealso href="https://docs.unity3d.com/Manual/DomainReloading.html"/>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            s_shared8Limit = new(8);
        }

        public CappedArrayPool(int maxLength)
        {
            _buckets = new T[maxLength][][];
            _tails = new int[maxLength];

            for (var i = 0; i < maxLength; i++)
            {
                var arrayLength = i + 1;
                _buckets[i] = new T[INITIAL_BUCKET_SIZE][];

                for (var j = 0; j < INITIAL_BUCKET_SIZE; j++)
                {
                    _buckets[i][j] = new T[arrayLength];
                }

                _tails[i] = 0;
            }
        }

        public T[] Rent(int length)
        {
            if (length <= 0)
                return Array.Empty<T>();

            if (length > _buckets.Length)
                return new T[length]; // Not supported

            var i = length - 1;

            lock (_syncRoot)
            {
                var bucket = _buckets[i];
                var tail = _tails[i];

                if (tail >= bucket.Length)
                {
                    Array.Resize(ref bucket, bucket.Length * 2);
                    _buckets[i] = bucket;
                }

                bucket[tail] ??= new T[length];

                var result = bucket[tail];
                _tails[i] += 1;
                return result;
            }
        }

        public void Return(T[] array)
        {
            if (array.Length <= 0 || array.Length > _buckets.Length)
                return;

            var i = array.Length - 1;

            lock (_syncRoot)
            {
                if (s_isTManaged)
                    Array.Clear(array, 0, array.Length);

                if (_tails[i] > 0)
                    _tails[i] -= 1;
            }
        }
    }
}