// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Buffers
{
    public struct BufferProviderEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        private readonly IBufferProvider<T> _list;
        private int _index;
        private readonly int _version;
        private T _current;

        internal BufferProviderEnumerator([NotNull] IBufferProvider<T> list)
        {
            _list = list;
            _index = 0;
            _version = list.Version;
            _current = default;
        }

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            var localList = _list;

            if (_version == localList.Version && ((uint)_index < (uint)localList.Count))
            {
                _current = localList.Buffer[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_version != _list.Version)
            {
                ThrowHelper.ThrowInvalidOperationException_EnumFailedVersion();
            }

            _index = _list.Count + 1;
            _current = default;
            return false;
        }

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        public void Reset()
        {
            if (_version != _list.Version)
            {
                ThrowHelper.ThrowInvalidOperationException_EnumFailedVersion();
            }

            _index = 0;
            _current = default;
        }

        readonly object IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _list.Count + 1)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumOpCantHappen();
                }

                return Current;
            }
        }
    }
}
