// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    public struct FasterListEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        private readonly FasterList<T> _list;
        private int _index;
        private readonly int _version;
        private T _current;

        internal FasterListEnumerator([NotNull] FasterList<T> list)
        {
            _list = list;
            _index = 0;
            _version = list._version;
            _current = default;
        }

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            var localList = _list;

            if (_version == localList._version && ((uint)_index < (uint)localList._count))
            {
                _current = localList._buffer[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (_version != _list._version)
            {
                ThrowHelper.ThrowInvalidOperationException_EnumFailedVersion();
            }

            _index = _list._count + 1;
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
            if (_version != _list._version)
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
                if (_index == 0 || _index == _list._count + 1)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumOpCantHappen();
                }

                return Current;
            }
        }
    }
}
