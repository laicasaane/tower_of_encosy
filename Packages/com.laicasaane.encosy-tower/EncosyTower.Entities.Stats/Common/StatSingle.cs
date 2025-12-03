using System.Runtime.CompilerServices;

namespace EncosyTower.Entities.Stats
{
    public readonly struct StatSingle<T>
    {
        public readonly T Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatSingle(T value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StatSingle<T>(T value)
            => new(value);
    }
}
