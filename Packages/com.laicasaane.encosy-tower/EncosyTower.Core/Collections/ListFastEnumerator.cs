// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Unsafe;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    public struct ListFastEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        private readonly ListExposed<T> _list;
        private int _index;
        private readonly int _version;
        private T _current;

        internal ListFastEnumerator([NotNull] ListExposed<T> list)
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

            if (_version == localList.Version && ((uint)_index < (uint)localList.Size))
            {
                _current = localList.Items[_index];
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

            _index = _list.Size + 1;
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
                if (_index == 0 || _index == _list.Size + 1)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumOpCantHappen();
                }

                return Current;
            }
        }
    }
}
