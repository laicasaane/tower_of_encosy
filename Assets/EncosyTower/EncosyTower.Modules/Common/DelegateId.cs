using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public readonly struct DelegateId : IEquatable<DelegateId>
    {
        public static readonly DelegateId Null = default;

        private readonly IntPtr _method;
        private readonly int _delegate;
        private readonly int _stateHash;

        public DelegateId(Delegate @delegate, int stateHash = 0) : this()
        {
            if (@delegate == null)
            {
                _method = IntPtr.Zero;
                _delegate = 0;
            }
            else
            {
                _method = @delegate.Method.MethodHandle.Value;
                _delegate = @delegate.GetHashCode();
            }

            _stateHash = stateHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{_delegate}+{_method}+{_stateHash}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DelegateId other)
            => _delegate == other._delegate && _method == other._method && _stateHash == other._stateHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => obj is DelegateId other && _delegate == other._delegate && _method == other._method && _stateHash == other._stateHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => HashCode.Combine(_delegate, _method, _stateHash);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DelegateId left, DelegateId right)
            => left._delegate == right._delegate && left._method == right._method && left._stateHash == right._stateHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DelegateId left, DelegateId right)
            => left._delegate != right._delegate || left._method != right._method || left._stateHash != right._stateHash;
    }
}
