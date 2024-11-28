using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public readonly record struct PresetId(Id Value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => Value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PresetId(Id value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PresetId(Id.Serializable value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id(PresetId value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Id.Serializable(PresetId value)
            => value.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Id2(PresetId value)
            => TypeId<PresetId>.Value.ToId2(value);
    }
}
