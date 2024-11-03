using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public sealed class ValueRef<T> where T : struct
    {
        public T Value { get; set; }

        public ValueRef()
        {
            Value = default;
        }

        public ValueRef(T value)
        {
            Value = value;
        }

        public override string ToString()
            => Value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(ValueRef<T> refT)
            => refT?.Value ?? default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ValueRef<T>(T value)
            => new(value);
    }
}
