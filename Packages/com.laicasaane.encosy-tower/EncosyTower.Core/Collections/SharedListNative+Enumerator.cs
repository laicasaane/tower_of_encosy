using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;

namespace EncosyTower.Collections
{
    partial struct SharedListNative<T, TNative>
    {
        public struct Enumerator : IEnumerator<TNative>, IEnumerator
        {
            private readonly ReadOnly _list;
            private int _index;
            private readonly int _version;
            private TNative _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ReadOnly list)
            {
                _list = list;
                _index = 0;
                _version = list._version[0];
                _current = default;
            }

            public bool MoveNext()
            {
                var localList = _list;

                if (_version == localList.Version && ((uint)_index < (uint)localList.Count))
                {
                    _current = localList._buffer[_index];
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

            public readonly TNative Current
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose()
            {
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
}
