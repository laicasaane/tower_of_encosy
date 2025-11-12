#if UNITY_COLLECTIONS && UNITY_MATHEMATICS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Debugging;
using Unity.Collections;

namespace EncosyTower.StringIds
{
    partial struct NativeStringVault
    {
        public struct Enumerator : IEnumerator<UnmanagedString>
        {
            private readonly NativeArray<Range>.ReadOnly _ranges;
            private readonly NativeArray<byte>.ReadOnly _buffer;

            private Range _current;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(
                  NativeArray<Range>.ReadOnly ranges
                , NativeArray<byte>.ReadOnly buffer
            )
            {
                _ranges = ranges;
                _buffer = buffer;
                _current = default;
                _index = 0;
            }

            public bool MoveNext()
            {
                var ranges = _ranges;

                if (((uint)_index < (uint)ranges.Length))
                {
                    _current = ranges[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _ranges.Length + 1;
                _current = default;
                return false;
            }

            public readonly UnmanagedString Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => UnmanagedString.FromBufferAt(_current, _buffer).GetValueOrThrow();
            }

            public void Reset()
            {
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
                    if (_index == 0 || _index == _ranges.Length + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException_EnumOpCantHappen();
                    }

                    return Current;
                }
            }
        }
    }
}

#endif
