#if UNITY_COLLECTIONS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Debugging;

namespace EncosyTower.StringIds
{
    partial class StringVault
    {
        public struct Enumerator : IEnumerator<UnmanagedString>
        {
            private readonly SharedListNative<Range>.ReadOnly _ranges;
            private readonly SharedListNative<byte>.ReadOnly _buffer;

            private Range _current;
            private readonly int _version;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(
                  SharedListNative<Range>.ReadOnly ranges
                , SharedListNative<byte>.ReadOnly buffer
            )
            {
                _ranges = ranges;
                _buffer = buffer;
                _version = ranges.Version;
                _current = default;
                _index = 0;
            }

            public bool MoveNext()
            {
                var ranges = _ranges;

                if (_version == ranges.Version && ((uint)_index < (uint)ranges.Count))
                {
                    _current = ranges[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _ranges.Version)
                {
                    ThrowHelper.ThrowInvalidOperationException_EnumFailedVersion();
                }

                _index = _ranges.Count + 1;
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
                if (_version != _ranges.Version)
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
                    if (_index == 0 || _index == _ranges.Count + 1)
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
